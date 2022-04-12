/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DarkRift.Server;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using DarkRift.Server.Configuration;

namespace DarkRift.Server.Console
{
    internal class Program
    {
        /// <summary>
        ///     The server instance.
        /// </summary>
        private static DarkRiftServer server;

        /// <summary>
        ///     Main entry point of the server which starts a single server.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            string[] rawArguments = CommandEngine.ParseArguments(string.Join(" ", args));
            string[] arguments = CommandEngine.GetArguments(rawArguments);
            NameValueCollection variables = CommandEngine.GetFlags(rawArguments);

            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
                variables.Add((string)environmentVariable.Key, (string)environmentVariable.Value);

            string serverConfigFile;
            string clusterConfigFile;
            if (arguments.Length < 1)
            {
                serverConfigFile = "Server.config";
                clusterConfigFile = "Cluster.config";
            }
            else if (arguments.Length == 1)
            {
                serverConfigFile = arguments[0];
                clusterConfigFile = "Cluster.config";
            }
            else if (arguments.Length == 2)
            {
                serverConfigFile = arguments[0];
                clusterConfigFile = arguments[1];
            }
            else
            {
                System.Console.Error.WriteLine("Unexpected number of comand line arguments passed. Expected 0-2 but found " + arguments.Length + ".");
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
                return;
            }

            DarkRiftServerConfigurationBuilder serverConfigurationBuilder;

            try
            {
                serverConfigurationBuilder = DarkRiftServerConfigurationBuilder.CreateFromXml(serverConfigFile, variables);
            }
            catch (IOException e)
            {
                System.Console.Error.WriteLine("Could not load the server config file needed to start (" + e.Message + "). Are you sure it's present and accessible?");
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
                return;
            }
            catch (XmlConfigurationException e)
            {
                System.Console.Error.WriteLine($"Failed to load '{serverConfigFile}': {e.Message}");
                System.Console.Error.WriteLine();
                System.Console.Error.WriteLine(e.DocumentationLink != null ? $"See {e.DocumentationLink} for more information." : "No additional documentation available.");
                System.Console.Error.WriteLine();
                System.Console.Error.WriteLine(e.LineInfo != null && e.LineInfo.HasLineInfo() ? $"Line {e.LineInfo.LineNumber} Col: {e.LineInfo.LinePosition}" : "(Unknown location)");
                System.Console.Error.WriteLine();
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
                return;
            }

            // Set this thread as the one executing dispatcher tasks
            serverConfigurationBuilder.WithDispatcherExecutorThreadID(Thread.CurrentThread.ManagedThreadId);

#if PRO
            if (File.Exists(clusterConfigFile))
            {
                DarkRiftClusterConfigurationBuilder clusterConfigurationBuilder;
                try
                {
                    clusterConfigurationBuilder = DarkRiftClusterConfigurationBuilder.CreateFromXml(clusterConfigFile, variables);
                }
                catch (IOException e)
                {
                    System.Console.Error.WriteLine("Could not load the cluster config file needed to start (" + e.Message + "). Are you sure it's present and accessible?");
                    System.Console.WriteLine("Press any key to exit...");
                    System.Console.ReadKey();
                    return;
                }
                catch (XmlConfigurationException e)
                {
                    System.Console.Error.WriteLine($"Failed to load '{clusterConfigFile}': {e.Message}");
                    System.Console.Error.WriteLine();
                    System.Console.Error.WriteLine(e.DocumentationLink != null ? $"See {e.DocumentationLink} for more information." : "No additional documentation available.");
                    System.Console.Error.WriteLine();
                    System.Console.Error.WriteLine(e.LineInfo != null && e.LineInfo.HasLineInfo() ? $"Line {e.LineInfo.LineNumber} Col: {e.LineInfo.LinePosition}" : "(Unknown location)");
                    System.Console.Error.WriteLine();
                    System.Console.WriteLine("Press any key to exit...");
                    System.Console.ReadKey();
                    return;
                }

                server = new DarkRiftServer(serverConfigurationBuilder.ServerSpawnData, clusterConfigurationBuilder.ClusterSpawnData);
            }
            else
            {
                server = new DarkRiftServer(serverConfigurationBuilder.ServerSpawnData);
            }
#else
            server = new DarkRiftServer(serverConfigurationBuilder.ServerSpawnData);
#endif


            server.StartServer();

            new Thread(new ThreadStart(ConsoleLoop)).Start();

            while (!server.Disposed)
            {
                server.DispatcherWaitHandle.WaitOne();
                server.ExecuteDispatcherTasks();
            }
        }

        /// <summary>
        ///     Invoked from another thread to repeatedly execute commands from the console.
        /// </summary>
        private static void ConsoleLoop()
        {
            while (!server.Disposed)
            {
                string input = System.Console.ReadLine();

                if (input == null)
                {
                    System.Console.WriteLine("Stopping input loop as we seem to be running without an input stream.");
                    return;
                }

                server.ExecuteCommand(input);
            }
        }
    }
}
