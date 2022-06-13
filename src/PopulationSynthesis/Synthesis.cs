/*
    Copyright 2021 Travel Modelling Group, Department of Civil Engineering, University of Toronto

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

namespace PopulationSynthesis;

/// <summary>
/// This class is used for controlling the running of the population synthesis algorithm.
/// </summary>
public static class Synthesis
{
    /// <summary>
    /// Execute the synthesis procedure.
    /// </summary>
    /// <param name="configuration">The configuration of the synthesis to execute.</param>
    /// <exception cref="Exception">Thrown when there is an error that forces the execution of the synthesis to terminate.</exception>
    public static void RunSynthesis(Configuration configuration)
    {
        var landUse = new LandUse(Path.Combine(configuration.InputDirectory, "ZoneSystem.csv"), configuration.PopulationForecastFile);
        var households = Household.ReadHouseholds(GetHouseholdsFile(configuration.InputDirectory));
        var persons = Person.ReadPersons(GetPersonsFile(configuration.InputDirectory));
        var pds = landUse.GetPlanningDistricts();
        var random = new Random(configuration.RandomSeed);
        var pdSeed = pds.Select(pd => (PD: pd, Seed: random.Next(0, int.MaxValue))).ToDictionary(v => v.PD, v => v.Seed);
        Record(pds.AsParallel()
                  .AsOrdered()
                  .SelectMany(pd => GeneratePopulation(landUse, households, pd, pdSeed[pd])),
                  configuration.OutputDirectory,
                  households,
                  persons);
    }

    /// <summary>
    /// Regenerate the worker categories from the output directory.
    /// </summary>
    /// <param name="configuration">The configuration of the synthesis to execute.</param>
    /// <exception cref="Exception">Thrown when there is an error that forces the execution of the synthesis to terminate.</exception>
    public static void RegenerateWorkerCategories(Configuration configuration)
    {
        var households = Household.ReadHouseholds(GetHouseholdsFile(configuration.OutputDirectory, "HouseholdData/Households.csv"));
        var persons = Person.ReadPersons(GetPersonsFile(configuration.OutputDirectory, "HouseholdData/Persons.csv"));
        // PDs are zones for this functionality
        var workers = new WorkerCategoryBuilder();
        foreach (var household in households.OrderBy(k => k.Key))
        {
            var householdMembers = persons[household.Key];
            var zone = household.Value.HouseholdPD;
            workers.Record(household.Value, householdMembers, zone);
        }
        workers.WriteResults(configuration.OutputDirectory);
    }

    /// <summary>
    /// Record the results of the population synthesis.
    /// </summary>
    /// <param name="synthesisResults">The picked households for each zone.</param>
    /// <param name="outputDirectory">The directory that we will be storing the results.</param>
    /// <param name="households">The seed households.</param>
    /// <param name="persons">The seed persons.</param>
    private static void Record(IEnumerable<(int householdId, int taz)> synthesisResults, string outputDirectory,
    Dictionary<int, Household> households, Dictionary<int, List<Person>> persons)
    {
        var householdData = CreateDirectory(outputDirectory, "HouseholdData");
        using var householdWriter = CreateStreamWriter(Path.Combine(householdData.FullName, "Households.csv"));
        using var personWriter = CreateStreamWriter(Path.Combine(householdData.FullName, "Persons.csv"));
        int householdId = 1;
        WriteHeaders(householdWriter, personWriter);
        var workers = new WorkerCategoryBuilder();
        foreach ((int householdIndex, int taz) in synthesisResults)
        {
            var household = households[householdIndex];
            var householdMembers = persons[householdIndex];
            WriteHousehold(householdWriter, household, householdId, taz);
            WritePersonRecords(personWriter, householdId, householdMembers);
            workers.Record(household, householdMembers, taz);
            householdId++;
        }
        workers.WriteResults(outputDirectory);
    }

    /// <summary>
    /// Emit the household record.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="household">The household object to clone.</param>
    /// <param name="householdId">The household id for this record.</param>
    /// <param name="taz">The TAZ that the household lives in.</param>
    private static void WriteHousehold(StreamWriter writer, Household household, int householdId, int taz)
    {
        writer.Write(householdId);
        writer.Write(',');
        writer.Write(taz);
        writer.Write(',');
        writer.Write(1);
        writer.Write(',');
        writer.Write(household.DwellingType);
        writer.Write(',');
        writer.Write(household.NumberOfPersons);
        writer.Write(',');
        writer.Write(household.NumberOfVehicles);
        writer.Write(',');
        writer.WriteLine(household.Income);
    }

    /// <summary>
    /// Emit the person records for the household.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="householdId">The household id for this household.</param>
    /// <param name="people">The people living in this household.</param>
    private static void WritePersonRecords(StreamWriter writer, int householdId, List<Person> people)
    {
        var averageExpansion = people.Average(p => p.ExpansionFactor);
        for (int i = 0; i < people.Count; i++)
        {
            var person = people[i];
            writer.Write(householdId);
            writer.Write(',');
            writer.Write(i + 1);
            writer.Write(',');
            writer.Write(person.Age);
            writer.Write(',');
            writer.Write(person.Sex);
            writer.Write(',');
            writer.Write(person.License);
            writer.Write(',');
            writer.Write(person.TransitPass);
            writer.Write(',');
            writer.Write(person.EmploymentStatus);
            writer.Write(',');
            writer.Write(person.Occupation);
            writer.Write(',');
            writer.Write(person.FreeParking);
            writer.Write(',');
            writer.Write(person.StudentStatus);
            writer.Write(',');
            writer.Write(person.EmploymentPD);
            writer.Write(',');
            writer.Write(person.SchoolPD);
            writer.Write(',');
            writer.WriteLine(person.ExpansionFactor / averageExpansion);
        }
    }

    /// <summary>
    /// Write the headers for the household and person files.
    /// </summary>
    /// <param name="householdWriter">The stream for the household writer.</param>
    /// <param name="personWriter">The stream for the person writer.</param>
    private static void WriteHeaders(StreamWriter householdWriter, StreamWriter personWriter)
    {
        householdWriter.WriteLine("HouseholdID,Zone,ExpansionFactor,DwellingType,NumberOfPersons,NumberOfVehicles,Income");
        personWriter.WriteLine("HouseholdID,PersonNumber,Age,Sex,License,TransitPass,EmploymentStatus,Occupation,FreeParking,StudentStatus,EmploymentZone,SchoolZone,ExpansionFactor");
    }

    /// <summary>
    /// Generate a selection of household records for the given population.
    /// </summary>
    /// <param name="landUse">Land-use information.</param>
    /// <param name="households">Household records indexed by householdId.</param>
    /// <returns>Returns the selections for households by each planning district.</returns>
    private static IEnumerable<(int householdId, int taz)> GeneratePopulation(LandUse landUse, Dictionary<int, Household> households,
        int pd, int randomSeed)
    {
        var pool = CreatePoolForPD(households, pd);
        var expFactors = new float[pool.Length];
        var zones = landUse.GetZonesInPlanningDistrict(pd);
        var remaining = zones.Select(zone => (int)Math.Round(landUse.GetPopulation(zone))).ToArray();
        var random = CreateRandomNumberGenerators(pd, randomSeed, zones.Count);
        var totalExpansionFactor = CopyExpansionFactors(expFactors, pool);
        const int numberOfAttempts = 3;
        // Keep drawing people until no zones required any new households
        var any = true;
        while (any)
        {
            any = false;
            for (int i = 0; i < zones.Count; i++)
            {
                if (remaining[i] > 0)
                {
                    any = true;
                    for (int attempt = 0; ; attempt++)
                    {
                        int index = Pick(expFactors, random[i], pool, ref totalExpansionFactor, ref remaining[i]);
                        // If we can't find a household in the pool then reset the expansion factors.
                        if (index < 0)
                        {
                            if (attempt < numberOfAttempts)
                            {
                                totalExpansionFactor = CopyExpansionFactors(expFactors, pool);
                            }
                            else
                            {
                                DiagnosePossibleIssues(zones[i], pd, expFactors, pool, remaining[i]);
                            }
                            continue;
                        }
                        yield return (pool[index].Key, zones[i]);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Go through the state of GeneratePopulation and
    /// attempt to figure out why the algorithm is not working.
    /// </summary>
    /// <param name="zoneNumber">The zone number that this failed on.</param>
    /// <param name="pd">The planning district that is currently being evaluated.</param>
    /// <param name="expFactors">The remaining expansionFactors.</param>
    /// <param name="pool">The pool of households to draw from.</param>
    /// <param name="remaining">The number of persons left to draw for the zone.</param>
    private static void DiagnosePossibleIssues(int zoneNumber, int pd, float[] expFactors, KeyValuePair<int, Household>[] pool, int remaining)
    {
        if (pool.Length == 0)
        {
            throw new SynthesisException($"Unable to select a household for zone {zoneNumber} because there are no seed households in the planning district {pd}!");
        }
        if (!pool.Any(entry => entry.Value.NumberOfPersons <= remaining))
        {
            throw new SynthesisException($"Unable to select a household for zone {zoneNumber} because there are no households in the seed records with at most {remaining} persons living in it!");
        }
        if (expFactors.Sum() == 0.0f)
        {
            throw new SynthesisException($"Unable to select a household for zone {zoneNumber} because the seed population for PD {pd} have no expansion factors!");
        }
        throw new SynthesisException($"Unable to select a household for zone {zoneNumber}!");
    }

    /// <summary>
    /// Pick a household with no more than the remaining size from the pool.
    /// </summary>
    /// <param name="expFactors"></param>
    /// <param name="random"></param>
    /// <param name="remaining"></param>
    /// <returns></returns>
    private static int Pick(float[] expFactors, Random random,
        KeyValuePair<int, Household>[] pool, ref float totalExpansionFactor, ref int remaining)
    {
        double acc = 0.0f;
        const float minExp = 0.01f;
        var pop = random.NextDouble() * totalExpansionFactor;
        for (int i = 0; i < expFactors.Length; i++)
        {
            acc += expFactors[i];
            if ((acc >= pop) & expFactors[i] > 0)
            {
                var householdSize = pool[i].Value.NumberOfPersons;
                if (householdSize <= remaining)
                {
                    remaining -= householdSize;
                    var original = expFactors[i];
                    expFactors[i] = expFactors[i] - 1.0f;
                    if (expFactors[i] < minExp)
                    {
                        expFactors[i] = 0.0f;
                    }
                    totalExpansionFactor -= original - expFactors[i];
                    return i;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// Initialize a random number generator per zone.
    /// </summary>
    /// <param name="pd">The planning district this is for.</param>
    /// <param name="randomSeed">The initial random seed.</param>
    /// <param name="size">The number of zones to generate a random generator for.</param>
    /// <returns>A random number generator for each zone.</returns>
    private static Random[] CreateRandomNumberGenerators(int pd, int randomSeed, int size)
    {
        var random = new Random(randomSeed * pd);
        var ret = new Random[size];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = new Random(random.Next());
        }
        return ret;
    }

    /// <summary>
    /// Copy the expansion factors from the pool into the expansionFactor vector.
    /// </summary>
    /// <param name="expFactors">The expFactor array to set.</param>
    /// <param name="pool">The pool to copy the expansion factors from.</param>
    private static float CopyExpansionFactors(float[] expFactors, KeyValuePair<int, Household>[] pool)
    {
        double acc = 0.0;
        for (int i = 0; i < pool.Length; i++)
        {
            acc += expFactors[i] = pool[i].Value.ExpansionFactor;
        }
        return (float)acc;
    }

    /// <summary>
    /// Creates a pool for a planning district of seed households to draw from given the full set of households
    /// </summary>
    /// <param name="households">The complete pool of all households</param>
    /// <param name="pd">The planning district to select for.</param>
    /// <returns>An array of households and id numbers that reside in the planning district.</returns>
    private static KeyValuePair<int, Household>[] CreatePoolForPD(Dictionary<int, Household> households, int pd)
    {
        return households
            .Where(entry => entry.Value.HouseholdPD == pd)
            .OrderBy(entry => entry.Key)
            .ToArray();
    }

    /// <summary>
    /// Get the Household.csv file given the path to the input population directory.
    /// </summary>
    /// <param name="inputPopulationDirectory">The directory containing the input population.</param>
    /// <returns>The path to the Household records.</returns>
    /// <exception cref="FileNotFoundException">Throws if the file does not exist.</exception>
    private static string GetHouseholdsFile(string inputPopulationDirectory, string innerName = "SeedHouseholds.csv") => GetFilePathOrThrow(Path.Combine(inputPopulationDirectory, innerName));

    /// <summary>
    /// Get the Persons.csv file given the path to the input population directory.
    /// </summary>
    /// <param name="inputPopulationDirectory">The directory containing the input population.</param>
    /// <returns>The path to the Persons records.</returns>
    /// <exception cref="FileNotFoundException">Throws if the file does not exist.</exception>
    private static string GetPersonsFile(string inputPopulationDirectory, string innerName = "SeedPersons.csv") => GetFilePathOrThrow(Path.Combine(inputPopulationDirectory, innerName));
}
