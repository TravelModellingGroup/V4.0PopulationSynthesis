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
/// Represents a the household level variables.
/// </summary>
/// <param name="HouseholdID">The unique identifier for the household.</param>
/// <param name="HouseholdPD">The planning district that the household lives in.</param>
/// <param name="ExpansionFactor">A factor that expands this record to match census totals.</param>
/// <param name="DwellingType">1 = Household, 2 = Apartment, 3 = Townhouse, 9 = Unknown</param>
/// <param name="NumberOfPersons">The number of persons living in the household.</param>
/// <param name="NumberOfVehicles">The number of vehicles that the household has.</param>
/// <param name="Income">The TTS income level for the household.</param>
public readonly record struct Household(int HouseholdID, int HouseholdPD, float ExpansionFactor,
    int DwellingType, int NumberOfPersons, int NumberOfVehicles, int Income)
{
    /// <summary>
    /// Load the household records from CSV.
    /// </summary>
    /// <param name="fileName">The path to the household file to load.</param>
    /// <returns>Returns a dictionary of household records indexed by householdId.</returns>
    public static Dictionary<int, Household> ReadHouseholds(string fileName)
    {
        return File.ReadLines(fileName)
            .Skip(1)
            .AsParallel()
            .AsOrdered()
            .Select(line => line.Split(','))
            .Where(parts => parts.Length == 7)
            .Select(parts => new Household(int.Parse(parts[0]), int.Parse(parts[1]), float.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6])))
            .ToDictionary(h => h.HouseholdID);
    }
}
