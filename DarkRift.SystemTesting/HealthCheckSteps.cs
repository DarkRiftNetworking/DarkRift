/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using static DarkRift.Server.DarkRiftInfo;

namespace DarkRift.SystemTesting
{
    /// <summary>
    ///     Steps for testing the server's health check
    /// </summary>
    [Binding]
    internal class HealthCheckSteps
    {
        /// <summary>
        ///     The world to store state in.
        /// </summary>
        private readonly World world;

        /// <summary>
        ///    The downloaded health check data.
        /// </summary>
        private string jsonString;

        public HealthCheckSteps(World world)
        {
            this.world = world;
        }

        [When("I query the health check port")]
        public void WhenIQueryTheHealthCheckPort()
        {
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync("http://localhost:10666/health").Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            jsonString = response.Content.ReadAsStringAsync().Result;
        }

        [Then("the server returns the expected fields")]
        public void ThenTheServerReturnsTheExpectedFields()
        {
            HealthCheckObject healthcheckObject = JsonConvert.DeserializeObject<HealthCheckObject>(jsonString);

            Assert.IsTrue(healthcheckObject.Listening, "Expected the health check to report the server is listening.");
            Assert.AreEqual(0, (world.GetServer(0).ServerInfo.StartTime - healthcheckObject.StartTime).TotalSeconds, 1, "Expected the health check start time to be within a second of the actual.");
            Assert.AreEqual(world.GetServer(0).ServerInfo.Type, healthcheckObject.Type, "Expected the health check to report the correct DarkRift tier.");
            Assert.AreEqual(world.GetServer(0).ServerInfo.Version, healthcheckObject.Version, "Expected the health check to report the correct DarkRift version.");
        }

        private class HealthCheckObject
        {
            public bool Listening { get; set; }
            public DateTime StartTime { get; set; }
            public ServerType Type { get; set; }
            public Version Version { get; set; }
        }
    }
}
