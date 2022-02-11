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
namespace PopulationSynthesis;

/// <summary>
/// This exception is thrown when there is an error in the synthesis procedure that can not be resolved.
/// </summary>
internal class SynthesisException : Exception
{
    /// <summary>
    /// Used for throwing a synthesis procedure exception with a message for the user.
    /// </summary>
    /// <param name="message">The error message to report to the user.</param>
    public SynthesisException(string message) : base(message) { }
}
