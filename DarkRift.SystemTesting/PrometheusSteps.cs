/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
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
    internal class PrometheusSteps
    {
        /// <summary>
        ///     The world to store state in.
        /// </summary>
        private readonly World world;

        /// <summary>
        ///    The downloaded Prometheus metric data.
        /// </summary>
        private string prometheusString;

        public PrometheusSteps(World world)
        {
            this.world = world;
        }

        [When("I query the Prometheus endpoint")]
        public void WhenIQueryThePrometheusEndpoint()
        {
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync("http://localhost:9796/metrics").Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            prometheusString = response.Content.ReadAsStringAsync().Result;
        }

        [Then("the server returns the metrics in (.*)")]
        public void ThenTheServerReturnsTheExpectedMetrics(string expectedMetricsFile)
        {
            // Assert line by line for better debugging
            string[] expectedLines = File.ReadAllLines(expectedMetricsFile);
            string[] actualLines = prometheusString.Split('\n');

            Assert.AreEqual(expectedLines.Length, actualLines.Length);
            for (int i = 0; i < expectedLines.Length; i++)
                Assert.AreEqual(expectedLines[i], actualLines[i], $"Expected line {i + 1} to match.");
        }

        [Then("the metric '(.*)' has value (.*)")]
        public void ThenTheMetricHasValue(string metricName, string value)
        {
            // Assert line by line for better debugging
            string[] lines = prometheusString.Split('\n');
            bool found = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(' ');
                Assert.AreEqual(2, parts.Length, $"Metric line '{line}' is invalid.");
                if (parts[0] == metricName)
                {
                    if (!found)
                        Assert.AreEqual(value, parts[1], $"Value for metric '{metricName}' was unexpected.");
                    else
                        Assert.Fail($"Duplicate metric '{metricName}' found.");

                    found = true;
                }
            }

            if (!found)
                Assert.Fail($"Metric '{metricName}' was not found.");
        }
    }
}
