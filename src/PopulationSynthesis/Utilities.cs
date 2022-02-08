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
/// Provides some common utilities for processing input data.
/// </summary>
internal static class Utilities
{
    /// <summary>
    /// Returns the full path to the file or throws if the files does not exist.
    /// </summary>
    /// <param name="filePath">The path of the file to search for.</param>
    /// <returns>The full path to the given file path.</returns>
    /// <exception cref="FileNotFoundException">Throws if the file does not exist.</exception>
    public static string GetFilePathOrThrow(string filePath)
    {
        FileInfo fileInfo = new(filePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(null, fileInfo.FullName);
        }
        return fileInfo.FullName;
    }

    /// <summary>
    /// Parses the integer from the given string placing the results in the value.  If it is unable to parse the value
    /// an error is reported with the column name.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <param name="value">The converted value of the string.</param>
    /// <param name="columnName">The name of the column that is being processed.</param>
    /// <param name="error">An error message filled out if the parse fails.</param>
    /// <returns>True if the parse succeeds, false otherwise with an error message stored in 'error'.</returns>
    public static bool ParseInt(string str, out int value, string columnName, ref string? error)
    {
        if (!int.TryParse(str, out value))
        {
            error = $"Unable to parse integer '{str}' in the column {columnName}";
            return false;
        }
        return true;
    }

    /// <summary>
    /// Parses the floating point value from the given string placing the results in the value.  If it is unable to parse the value
    /// an error is reported with the column name.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <param name="value">The converted value of the string.</param>
    /// <param name="columnName">The name of the column that is being processed.</param>
    /// <param name="error">An error message filled out if the parse fails.</param>
    /// <returns>True if the parse succeeds, false otherwise with an error message stored in 'error'.</returns>
    public static bool ParseFloat(string str, out float value, string columnName, ref string? error)
    {
        if (!float.TryParse(str, out value))
        {
            error = $"Unable to parse floating point number '{str}' in the column {columnName}";
            return false;
        }
        return true;
    }
}
