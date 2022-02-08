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
        var landUse = new LandUse(configuration.ZoneSystemFile, configuration.PopulationForecastFile);
        var households = Household.ReadHouseholds(GetHouseholdsFile(configuration.InputPopulationDirectory));
        var persons = Person.ReadPersons(GetPersonsFile(configuration.InputPopulationDirectory));
        var pds = landUse.GetPlanningDistricts();
        var random = new Random(configuration.RandomSeed);
        var pdSeed = pds.Select(pd => (PD: pd, Seed: random.Next(0, int.MaxValue))).ToDictionary(v => v.PD, v => v.Seed);
        Record(pds.AsParallel()
                  .AsOrdered()
                  .SelectMany(pd => GeneratePopulation(landUse, households, persons, pd, pdSeed[pd])),
                  configuration.OutputDirectory,
                  households,
                  persons);
    }

    /// <summary>
    /// Record the results of the population synthesis.
    /// </summary>
    /// <param name="synthesisResults"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="households"></param>
    /// <param name="persons"></param>
    private static void Record(ParallelQuery<(int householdId, int taz)> synthesisResults, string outputDirectory,
        Dictionary<int, Household> households, Dictionary<int, List<Person>> persons)
    {
        using var householdWriter = new StreamWriter(Path.Combine(outputDirectory, "Households.csv"));
        using var personWriter = new StreamWriter(Path.Combine(outputDirectory, "Persons.csv"));
        int householdId = 1;
        WriteHeaders(householdWriter, personWriter);
        foreach ((int householdIndex, int taz) in synthesisResults)
        {
            var household = households[householdIndex];
            WriteHousehold(householdWriter, household, householdId, taz);
            WritePersons(personWriter, householdId, persons[householdIndex]);
            householdId++;
        }
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
        writer.Write(household.ExpansionFactor);
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
    private static void WritePersons(StreamWriter writer, int householdId, List<Person> people)
    {
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
            writer.Write(person.EmploymentPD);
            writer.Write(',');
            writer.Write(person.SchoolPD);
            writer.Write(',');
            writer.WriteLine(person.ExpansionFactor);
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
    /// <param name="persons">Lists of persons living in households indexed by householdId.</param>
    /// <returns>Returns the selections for households by each planning district.</returns>
    private static IEnumerable<(int householdId, int taz)> GeneratePopulation(LandUse landUse, Dictionary<int, Household> households,
        Dictionary<int, List<Person>> persons, int pd, int randomSeed)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the Household.csv file given the path to the input population directory.
    /// </summary>
    /// <param name="inputPopulationDirectory">The directory containing the input population.</param>
    /// <returns>The path to the Household records.</returns>
    /// <exception cref="FileNotFoundException">Throws if the file does not exist.</exception>
    private static string GetHouseholdsFile(string inputPopulationDirectory) => GetFilePathOrThrow(Path.Combine(inputPopulationDirectory, "Households.csv"));

    /// <summary>
    /// Get the Persons.csv file given the path to the input population directory.
    /// </summary>
    /// <param name="inputPopulationDirectory">The directory containing the input population.</param>
    /// <returns>The path to the Persons records.</returns>
    /// <exception cref="FileNotFoundException">Throws if the file does not exist.</exception>
    private static string GetPersonsFile(string inputPopulationDirectory) => GetFilePathOrThrow(Path.Combine(inputPopulationDirectory, "Persons.csv"));
}
