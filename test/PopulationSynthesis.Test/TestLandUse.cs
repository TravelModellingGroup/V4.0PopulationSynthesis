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
using System;

namespace PopulationSynthesis.Test;

/// <summary>
/// This class contains the test related to land-use data
/// </summary>
[TestClass]
public class TestLandUse
{
    /// <summary>
    /// Test that we are able to load valid land-use data
    /// </summary>
    [TestMethod]
    public void LoadLandUse()
    {
        LandUse _ = new(TestZonePath, TestPopulationPath);
    }

    /// <summary>
    /// Test that we are unable to load invalid land-use data where there is a zone defined in
    /// the population files that is not in the zone system.
    /// </summary>
    [TestMethod]
    public void LoadInvalidLandUse()
    {
        Assert.ThrowsException<Exception>(() => new LandUse(TestZonePath, TestInvalidPopulationPath));
    }

    /// <summary>
    /// Make sure that we are able to load and get the planning districts correctly
    /// </summary>
    [TestMethod]
    public void GetPlanningDistricts()
    {
        LandUse landUse = new(TestZonePath, TestPopulationPath);
        // [1, 4, 5, 6]
        var pds = landUse.GetPlanningDistricts();
        Assert.IsNotNull(pds);
        Assert.AreEqual(4, pds.Length);
    }

    /// <summary>
    /// Test that we have the correct number of zones in each planning district
    /// </summary>
    [TestMethod]
    public void CorrectZonesInPDs()
    {
        LandUse landUse = new(TestZonePath, TestPopulationPath);
        var pds = landUse.GetPlanningDistricts();
        Assert.IsNotNull(pds);
        Assert.AreEqual(4, pds.Length);
        /*
          Zone,PD
          1,1
          2,1
          3,1
          41,4
          42,4
          51,5
          52,5
          61,6
        */
        void TestNumberOfZones(int pd, int expected)
        {
            var zones = landUse.GetZonesInPlanningDistrict(pd);
            Assert.IsNotNull(zones, $"We were unable to get the zones for PD {pd}!");
            Assert.AreEqual(expected, zones.Count, $"The number of zones in PD {pd} are incorrect. Expected {expected}, but found {zones.Count}!");
        }
        TestNumberOfZones(1, 3);
        TestNumberOfZones(4, 2);
        TestNumberOfZones(5, 2);
        TestNumberOfZones(6, 1);
        Assert.ThrowsException<Exception>(() => landUse.GetZonesInPlanningDistrict(0));
    }

    /// <summary>
    /// Test that we are able to get the correct population by zone.
    /// </summary>
    [TestMethod]
    public void GetPopulation()
    {
        LandUse landUse = new(TestZonePath, TestPopulationPath);
        /*
        Zone,Population
        1,5
        2,6
        3,7
        41,105
        42,106
        51,201
        52,202
        61,0
         */
        Assert.AreEqual(5.0f, landUse.GetPopulation(1), 0.00001f);
        Assert.AreEqual(6.0f, landUse.GetPopulation(2), 0.00001f);
        Assert.AreEqual(7.0f, landUse.GetPopulation(3), 0.00001f);
        Assert.AreEqual(202.0f, landUse.GetPopulation(52), 0.00001f);
        // Check for a zone that does not exist in the zone system
        Assert.ThrowsException<Exception>(() => landUse.GetPopulation(-1));
    }

    private static string TestZonePath => Path.Combine("TestData", "Zones.csv");

    private static string TestPopulationPath => Path.Combine("TestData", "Population.csv");
    
    private static string TestInvalidPopulationPath => Path.Combine("TestData", "InvalidPopulation.csv");
}
