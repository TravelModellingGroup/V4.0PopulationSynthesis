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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PopulationSynthesis;
using System.IO;

namespace PopulationSynthesis.Test;

/// <summary>
/// This class contains the test related to household records
/// </summary>
[TestClass]
public class TestHouseholds
{
    [TestMethod]
    public void LoadHouseholds()
    {
        var households = Household.ReadHouseholds(Path.Combine("TestData","Households.csv"));
        Assert.AreEqual(3, households.Count);
        Assert.IsTrue(households.ContainsKey(1));
        Assert.IsTrue(households.ContainsKey(2));
        Assert.IsTrue(households.ContainsKey(3));
        Assert.IsFalse(households.ContainsKey(4));
    }
}
