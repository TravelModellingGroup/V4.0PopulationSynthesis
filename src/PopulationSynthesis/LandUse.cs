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
/// Provides information about the LandUse for TAZ
/// </summary>
public sealed class LandUse
{
    /// <summary>
    /// Provides a mapping between TAZ to its planning district
    /// </summary>
    private readonly Dictionary<int, int> _zoneToPD = new();

    /// <summary>
    /// Provides a list of zones that exist within a planning district
    /// </summary>
    private readonly Dictionary<int, List<int>> _pdToZones = new();

    /// <summary>
    /// Provides the population by TAZ that we will need to generate
    /// </summary>
    private readonly Dictionary<int, float> _zoneToPopulation = new();

    /// <summary>
    /// Load in land-use information
    /// </summary>
    /// <param name="zoneSystemFile">The file path to the zone system file (TAZ,PD).</param>
    /// <param name="populationFile">The file path to the population file (TAZ,Pop) for the forecast.</param>
    /// <exception cref="Exception">Throws an exception if the land-use inputs are invalid.</exception>
    public LandUse(string zoneSystemFile, string populationFile)
    {
        LoadZoneSystem(zoneSystemFile);
        LoadPopulation(populationFile);
        EnsurePopulationZonesExists();
    }

    /// <summary>
    /// Load in the TAZ to Planning district mapping from CSV in the format of (TAZ,PD).
    /// </summary>
    /// <param name="zoneSystemFile">The path to the CSV file containing the forecast population.</param>
    /// <exception cref="Exception">Throws an exception when it is encounters an entry that can not be parsed.</exception>
    private void LoadZoneSystem(string zoneSystemFile)
    {
        foreach (var entries in File.ReadAllLines(zoneSystemFile)
            .Skip(1)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 2))
        {
            if (int.TryParse(entries[0], out var taz)
                && int.TryParse(entries[1], out var pd))
            {
                _zoneToPD[taz] = pd;
                // Make sure that the pd exists
                if (!_pdToZones.TryGetValue(pd, out var tazList))
                {
                    _pdToZones[pd] = tazList = new List<int>();
                }
                tazList.Add(taz);
            }
            else
            {
                throw new Exception($"Unable to read entry {entries[0]},{entries[1]} while reading the file {zoneSystemFile}!");
            }
        }
    }

    /// <summary>
    /// Load in the population from CSV in the format of (TAZ,Population)
    /// </summary>
    /// <param name="populationFile">The path to the CSV file containing the forecast population.</param>
    /// <exception cref="Exception">Throws an exception if there exists an invalid entry</exception>
    private void LoadPopulation(string populationFile)
    {
        foreach (var entries in File.ReadAllLines(populationFile)
            .Skip(1)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 2))
        {
            if (int.TryParse(entries[0], out var taz)
                && float.TryParse(entries[1], out var population))
            {
                _zoneToPopulation[taz] = population;
            }
            else
            {
                throw new Exception($"Unable to read entry {entries[0]},{entries[1]} while reading the file {populationFile}!");
            }
        }
    }

    /// <summary>
    /// Check to see if there is any zone with population defined that is not contained in the zone system.
    /// </summary>
    /// <exception cref="Exception">Thrown when there is a TAZ with population defined but is not included in the zone system.</exception>
    private void EnsurePopulationZonesExists()
    {
        // Check for a zone with population that does not exist in the zone to pd map
        if (_zoneToPopulation.Keys.Any(zone => !_zoneToPD.ContainsKey(zone)))
        {
            var violatingTAZ = _zoneToPopulation.Keys.First(zone => !_zoneToPD.ContainsKey(zone));
            throw new Exception($"The TAZ {violatingTAZ} with population but is not defined in the ZoneSystem file!");
        }
    }

    /// <summary>
    /// Get the planning districts in the zone system.
    /// </summary>
    /// <returns>An array of planning districts in the zone system.</returns>
    public int[] GetPlanningDistricts()
    {
        return _pdToZones.Keys.OrderBy(taz => taz).ToArray();
    }

    /// <summary>
    /// Get a list of zones that are contained within the planning district.
    /// </summary>
    /// <param name="planningDistrict">The planning district to get.</param>
    /// <returns>The list of zones contained in the planning district.</returns>
    /// <exception cref="Exception">Throws an exception if the planning district does not exist.</exception>
    public List<int> GetZonesInPlanningDistrict(int planningDistrict)
    {
        if(_pdToZones.TryGetValue(planningDistrict, out var list))
        {
            return list;
        }
        throw new Exception($"Unknown Planning district {planningDistrict} requested.");
    }

    /// <summary>
    /// Get the population from zone.
    /// </summary>
    /// <param name="zoneNumber">The zone number to get the population from.</param>
    /// <returns>The population for the given zone number.</returns>
    public float GetPopulation(int zoneNumber)
    {
        if(_zoneToPopulation.TryGetValue(zoneNumber, out var population))
        {
            return population;
        }
        else if(_zoneToPD.ContainsKey(zoneNumber))
        {
            return 0f;
        }
        else
        {
            throw new Exception($"The zone number {zoneNumber} does not exist in the zone system!");
        }
    }
}

