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
/// Provides a central object containing all of the settings that will be fed into the simulation.
/// </summary>
/// <param name="PopulationForecastFile">The path containing the forecast population by TAZ.</param>
/// <param name="ZoneSystemFile">The path containing the mapping between TAZ and PD.</param>
/// <param name="InputPopulationDirectory">The directory containing the input Households.csv and Persons.csv files.</param>
/// <param name="OutputDirectory">The directory to write all of the outputs into.</param>
/// <param name="RandomSeed">A set random seed to control the random number generator.</param>
public record Configuration(
    string PopulationForecastFile, string ZoneSystemFile, string InputPopulationDirectory, string OutputDirectory, int RandomSeed
);
