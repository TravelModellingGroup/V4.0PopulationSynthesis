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
using System.Linq;
using System.IO;


namespace PopulationSynthesis.Test;

/// <summary>
/// This class contains the test related to household records
/// </summary>
[TestClass]
public class TestPersons
{
    [TestMethod]
    public void LoadPersons()
    {
        var personsInHouseholds = Person.ReadPersons(Path.Combine("TestData","Persons.csv"));
        // There are actually 4 persons in the test data, but there are 3 households with data.
        Assert.AreEqual(3, personsInHouseholds.Count);
        // Now actually count all of the people.
        Assert.AreEqual(4, personsInHouseholds.Values.Sum(h => h.Count));
        Assert.IsTrue(personsInHouseholds.ContainsKey(1));
        Assert.IsTrue(personsInHouseholds.ContainsKey(2));
        Assert.IsTrue(personsInHouseholds.ContainsKey(3));
        Assert.IsFalse(personsInHouseholds.ContainsKey(4));
    }
}
