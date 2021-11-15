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

namespace PopulationSynthesis;
using static Utilities;

/// <summary>
/// Represents a person within a household.
/// </summary>
/// <param name="Age">The age of the person.</param>
/// <param name="Sex">The sex of the person, M or F.</param>
/// <param name="License">The driver's license status "Y" or "N".</param>
/// <param name="TransitPass">The transit pass status.</param>
/// <param name="EmploymentStatus">The employment status for the person. F, P, H, J, and
/// O are Full-Time, Part-Time, Full-Time-Work-At-Home, Part-Time-Work-At-Home, and unemployed respectively.</param>
/// <param name="Occupation">The occupation status for the person.  P, G, S, M, and
/// O are Professional, General Office / Clerical, Sales/Retail, Manufacturing / Construction, and Unemployed respectively.</param>
/// <param name="FreeParking">Does the person have free parking at work.  "Y" for yes or "N" for no.</param>
/// <param name="StudentStatus">"F" for Full-Time, "P" for Part-Time, or "O" for not a student.</param>
/// <param name="EmploymentPD">The planning district number that this person is employed in, 0 if they are not employed, or 8888 if they have no fixed place of work.</param>
/// <param name="SchoolPD">The planning district number that this person goes to school in, 0 if they are not a student.</param>
/// <param name="ExpansionFactor">The expansion for the record to total population used for this person.</param>
public readonly record struct Person(int Age, string Sex, string License, string TransitPass, string EmploymentStatus,
    string Occupation, bool FreeParking, string StudentStatus, int EmploymentPD, int SchoolPD, float ExpansionFactor)
{
    /// <summary>
    /// Loads in a dictionary of person records with the key of the household id.
    /// </summary>
    /// <param name="personsFile">The full file path to the Persons.csv</param>
    /// <returns>A dictionary of person records as lists organized by household.</returns>
    public static Dictionary<int, List<Person>> ReadPersons(string personsFile)
    {
        Dictionary<int, List<Person>> personsInHousehold = new();
        foreach (var record in File.ReadAllLines(personsFile)
            .Skip(1)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 13))
        {
            string? error = null;
            if (!int.TryParse(record[0], out var hid))
            {
                throw new Exception("Unable to process a household Id of ");
            }
            var persons = GetPersonsListOrCreate(personsInHousehold, hid);
            if (!AddPersonRecordToPersonsList(persons, record, ref error))
            {
                throw new Exception(error! + "\r\n" + string.Join(',', record));
            }
        }
        return personsInHousehold;
    }

    /// <summary>
    /// Get the household list from the ID of the household or creates a new one and appends it to the 
    /// </summary>
    /// <param name="personsInHousehold">A dictionary where the key is the household id and value of a list of persons living within it.</param>
    /// <param name="householdId">The household ID number</param>
    /// <returns>The list associated with the</returns>
    private static List<Person> GetPersonsListOrCreate(Dictionary<int, List<Person>> personsInHousehold, int householdId)
    {
        if (!personsInHousehold.TryGetValue(householdId, out List<Person>? persons))
        {
            personsInHousehold[householdId] = persons = new List<Person>();
        }
        return persons;
    }

    /// <summary>
    /// Loads in a person record and appends it to the list of persons within the household.
    /// </summary>
    /// <param name="persons"></param>
    /// <param name="record"></param>
    /// <param name="error"></param>
    private static bool AddPersonRecordToPersonsList(List<Person> persons, string[] record, ref string? error)
    {
        if (!ParseInt(record[2], out var age, "Age", ref error)) return false;
        string sex = record[3];
        string license = record[4];
        string transitPass = record[5];
        string employmentStatus = record[6];
        string occupation = record[7];
        bool freeParking = record[8] == "Y";
        string studentStatus = record[9];
        if (!ParseInt(record[10], out int employmentPD, "EmploymentPD", ref error)) return false;
        if (!ParseInt(record[11], out int schoolPD, "SchoolPD", ref error)) return false;
        if (!ParseFloat(record[12], out float expansionFactor, "ExpansionFactor", ref error)) return false;
        persons.Add(new Person(age, sex, license, transitPass, employmentStatus, occupation, freeParking, studentStatus, employmentPD, schoolPD, expansionFactor));
        return true;
    }
}

