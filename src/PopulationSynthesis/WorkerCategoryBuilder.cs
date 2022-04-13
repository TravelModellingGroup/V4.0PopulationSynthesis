/*
    Copyright 2022 Travel Modelling Group, Department of Civil Engineering, University of Toronto

    This file is part of V4.0PopulationSynthesis.

    V4.0PopulationSynthesis is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    V4.0PopulationSynthesis is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with V4.0PopulationSynthesis.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PopulationSynthesis;

/// <summary>
/// This class is designed to facilitate the recording of zonal residence and worker category vectors
/// generated from population synthesis.
/// </summary>
internal sealed class WorkerCategoryBuilder
{
    /// <summary>
    /// Keyed by zone number, [occEmpIndex * 3 + workerCategory]
    /// </summary>
    private readonly Dictionary<int, float[]> _data = new();

    private const int _numberOfOccupations = 4;
    private const int _numberOfOccEmp = _numberOfOccupations * 2;
    private const int _numberOfWorkerCategories = 3;

    /// <summary>
    /// Store the results of the household in our local data set.
    /// </summary>
    /// <param name="household">The household to store the results for</param>
    /// <param name="persons">The persons that belong to this household.</param>
    /// <param name="zoneNumber">The zone number that this household belongs to.</param>
    public void Record(Household household, List<Person> persons, int zoneNumber)
    {
        var licenses = NumberOfLicenses(persons);
        var wIndex = GetWorkerCategoryIndex(licenses, household.NumberOfVehicles);
        foreach (var person in persons)
        {
            var occEmpIndex = GetOccEmpIndex(person);
            if(occEmpIndex >= 0)
            {
                Increment(zoneNumber, occEmpIndex, wIndex, household.ExpansionFactor);
            }
        }
    }

    /// <summary>
    /// Stores the results of a person to the data set
    /// </summary>
    /// <param name="zoneNumber">The household's zone number.</param>
    /// <param name="occEmpIndex">The persons' occupation employment index.</param>
    /// <param name="wIndex">The worker category index.</param>
    /// <param name="expansionFactor">The household's expansion factor.</param>
    private void Increment(int zoneNumber, int occEmpIndex, int wIndex, float expansionFactor)
    {
        if(!_data.TryGetValue(zoneNumber, out var vector))
        {
            vector = new float[_numberOfWorkerCategories * _numberOfOccEmp];
            _data[zoneNumber] = vector;
        }
        vector[occEmpIndex * _numberOfWorkerCategories + wIndex] += expansionFactor;
    }

    /// <summary>
    /// Returns the worker's mobility category for the household.
    /// </summary>
    /// <param name="licenses">The number of licenses in the household.</param>
    /// <param name="vehicles">The number of vehicles in the households.</param>
    /// <returns>The worker category for the household.</returns>
    private static int GetWorkerCategoryIndex(int licenses, int vehicles)
    {
        if ((vehicles == 0) | (licenses == 0))
        {
            return 0;
        }
        return vehicles < licenses ? 1 : 2;
    }

    /// <summary>
    /// Gets the number of licenses in the household.
    /// </summary>
    /// <param name="persons">The people that live in the household.</param>
    /// <returns>The number of licenses in the household.</returns>
    private static int NumberOfLicenses(List<Person> persons)
    {
        return persons.Count(p => p.HasLicense());
    }

    /// <summary>
    /// Gets the occupation employment index for the person.
    /// </summary>
    /// <param name="person">The person to get the occupation employment index for.</param>
    /// <returns>The occupation employment index, -1 if the person is not employed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int GetOccEmpIndex(Person person)
    {
        var occ = GetOcc(person);
        int emp = GetEmp(person);
        return (occ >= 0) & (emp >= 0) ? 
            occ + emp * _numberOfOccupations 
            : -1;
    }

    /// <summary>
    /// Gets the occupation offset for the person.
    /// </summary>
    /// <param name="person">The person to get the occupation offset for.</param>
    /// <returns>The person's occupation offset, -1 if they do not have an occupation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int GetOcc(Person person)
    {
        return person.Occupation switch
        {
            "P" => 0,
            "G" => 1,
            "S" => 2,
            "M" => 3,
            _ => -1
        };
    }

    /// <summary>
    /// Gets the employment offset for the person.
    /// </summary>
    /// <param name="person">The person to get the employment offset for.</param>
    /// <returns>The person's employment offset, -1 if they are not employed out of home.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int GetEmp(Person person)
    {
        return person.EmploymentStatus switch
        {
            "F" => 0,
            "P" => 1,
            _ => -1
        };
    }

    /// <summary>
    /// Store the results of the worker category builder to a directory.
    /// </summary>
    /// <param name="directoryPath">The directory to store the results into.</param>
    public void WriteResults(string directoryPath)
    {
        var zonalResidence = CreateDirectory(directoryPath, "ZonalResidence");
        var workerCategories = CreateDirectory(directoryPath, "WorkerCategories");
        Parallel.Invoke(
            () => WriteZonalResidence(zonalResidence),
            () => WriteWorkerCategories(workerCategories)
        );
    }

    /// <summary>
    /// File names for zonal residence and worker categories
    /// </summary>
    private readonly string[] _occEmpFileName = new[] { "PF.csv", "GF.csv", "SF.csv", "MF.csv", "PP.csv", "GP.csv", "SP.csv", "MP.csv" };

    /// <summary>
    /// Write out the zonal residence files.
    /// </summary>
    /// <param name="directory">The directory to write them into.</param>
    private void WriteZonalResidence(DirectoryInfo directory)
    {
        Parallel.For(0, _numberOfOccEmp, (int occEmpIndex) =>
        {
            using var writer = new StreamWriter(Path.Combine(directory.FullName, _occEmpFileName[occEmpIndex]));
            WriteZonalResidence(writer, occEmpIndex);
        });
    }

    /// <summary>
    /// Write out a zonal residence file for the given occupation employment index
    /// </summary>
    /// <param name="writer">The stream to store the results into.</param>
    /// <param name="occEmpIndex">The occupationIndex to work with.</param>
    private void WriteZonalResidence(StreamWriter writer, int occEmpIndex)
    {
        writer.WriteLine("HomeZone,WorkerCategory,Data");
        foreach (var entry in _data
            .OrderBy(entry => entry.Key))
        {
            var zone = entry.Key;
            var data = entry.Value;
            var acc = 0.0f;
            for (int wc = 0; wc < _numberOfWorkerCategories; wc++)
            {
                acc += data[occEmpIndex * _numberOfWorkerCategories + wc];
            }
            writer.Write(zone);
            writer.Write(',');
            writer.WriteLine(acc);
        }
    }

    /// <summary>
    /// Write out the worker category files to the given directory.
    /// </summary>
    /// <param name="directory">The directory to write the worker category files into.</param>
    private void WriteWorkerCategories(DirectoryInfo directory)
    {
        Parallel.For(0, _numberOfOccEmp, (int occEmpIndex) =>
        {
            using var writer = CreateStreamWriter(Path.Combine(directory.FullName, _occEmpFileName[occEmpIndex]));
            WriteWorkerCategory(writer, occEmpIndex);
        });
    }

    /// <summary>
    /// Store the results of the worker categories for the given occEmpIndex
    /// </summary>
    /// <param name="writer">The writer to store the results into.</param>
    /// <param name="occEmpIndex">The occupationIndex to work with.</param>
    private void WriteWorkerCategory(StreamWriter writer, int occEmpIndex)
    {
        writer.WriteLine("HomeZone,WorkerCategory,Data");
        foreach (var entry in _data
            .OrderBy(entry => entry.Key))
        {
            var zone = entry.Key;
            var data = entry.Value;
            float invTotal = GetTheInvertedTotalEmployment(occEmpIndex, data);
            // Write out the ratios if they are non-zero.
            for (int wc = 0; wc < _numberOfWorkerCategories; wc++)
            {
                var result = data[occEmpIndex * _numberOfWorkerCategories + wc] * invTotal;
                if (result > 0.0f)
                {
                    writer.Write(zone);
                    writer.Write(',');
                    writer.Write(wc + 1);
                    writer.Write(',');
                    writer.WriteLine(result);
                }
            }
        }
    }

    /// <summary>
    /// Gets the inverted total of employment for a given zone and occupation employment index.
    /// </summary>
    /// <param name="occEmpIndex">The occupation employment index to lookup.</param>
    /// <param name="data">The row of worker category data for a given zone.</param>
    /// <returns>The inverted total of the zone's employment for the occupation
    /// employment category. 0 if there was no data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static float GetTheInvertedTotalEmployment(int occEmpIndex, float[] data)
    {
        var invTotal = 0.0f;
        // Figure out our totals and then invert it to write out the ratios
        for (int wc = 0; wc < _numberOfWorkerCategories; wc++)
        {
            invTotal += data[occEmpIndex * _numberOfWorkerCategories + wc];
        }
        invTotal = 1 / invTotal;
        // If we have an invalid total (no entries) then the answers should all be zero
        return !float.IsInfinity(invTotal) ? invTotal : 0.0f;
    }
}
