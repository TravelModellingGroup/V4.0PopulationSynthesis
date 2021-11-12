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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static PopulationSynthesis.Utilities;
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
        WriteHeaders(householdWriter, personWriter);
        foreach ((int householdId, int taz) in synthesisResults)
        {

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
