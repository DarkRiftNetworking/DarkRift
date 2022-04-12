/*
Copyright (c) 2022 Unordinal AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using System.Xml.Linq;

/// <summary>
///     Importer for importing DarkRift builds into the Unity project.
/// </summary>
public class DarkRiftBuildImporter
{
    /// <summary>
    ///     The location of the DarkRift root folder.
    /// </summary>
    private const string ROOT_DIR = "../";

    /// <summary>
    ///     Imports the debug files into the project.
    /// </summary>
    [MenuItem("DarkRift/Import Debug")]
    private static void ImportDebug()
    {
        Import("Debug", true, true);
    }

    /// <summary>
    ///     Imports the debug files into the project.
    /// </summary>
    [MenuItem("DarkRift/Import Free")]
    private static void ImportFree()
    {
        Import("Free", false, false);
    }

    /// <summary>
    ///     Imports the debug files into the project.
    /// </summary>
    [MenuItem("DarkRift/Import Pro")]
    private static void ImportPro()
    {
        Import("Pro", true, false);
    }

    /// <summary>
    ///     Imports the given configuration into the project.
    /// </summary>
    private static void Import(string configuration, bool importProAssets, bool importDebugFiles)
    {
        Clean();

        bool[] success = new bool[14];

        if (!ConfigurationExists(configuration))
        {
            EditorUtility.DisplayDialog("DarkRift Import Failed", "Configuration " + configuration + " has not been built yet.", "OK");
            return;
        }

        success[0] = CopyAndVerifyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.dll"), @"DarkRift\Plugins\DarkRift.dll");
        success[1] = CopyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.xml"), @"DarkRift\Plugins\DarkRift.xml");

        success[2] = CopyAndVerifyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.Server.dll"), @"DarkRift\Plugins\Server\DarkRift.Server.dll");
        success[3] = CopyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.Server.xml"), @"DarkRift\Plugins\Server\DarkRift.Server.xml");

        success[4] = CopyAndVerifyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.Client.dll"), @"DarkRift\Plugins\Client\DarkRift.Client.dll");
        success[5] = CopyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.Client.xml"), @"DarkRift\Plugins\Client\DarkRift.Client.xml");

        success[6] = CopyFile(CompilePath(configuration, "net4.0", "DarkRift Server.zip"), @"DarkRift Server (.NET Framework 4.0).zip");

        if (importProAssets)
        {
            success[7] = CopyFile(CompilePath(configuration, "netcoreapp2.0", "DarkRift Server.zip"), @"DarkRift Server (.NET Core 2.0).zip");
            success[8] = CopyFile(CompilePath(configuration, "netcoreapp3.1", "DarkRift Server.zip"), @"DarkRift Server (.NET Core 3.1).zip");
            success[9] = CopyFile(CompilePath(configuration, "net5.0", "DarkRift Server.zip"), @"DarkRift Server (.NET 5.0).zip");
            success[10] = CopyFile(CompilePath("DarkRift Source.zip"), @"DarkRift Source.zip");
        }
        else
        {
            success[7] = true;
            success[8] = true;
            success[9] = true;
            success[10] = true;
        }

        if (importDebugFiles)
        {
            success[11] = CopyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.pdb"), @"DarkRift\Plugins\DarkRift.pdb");
            success[12] = CopyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.Server.pdb"), @"DarkRift\Plugins\Server\DarkRift.Server.pdb");
            success[13] = CopyFile(CompilePath(configuration, "net4.0", @"Lib\DarkRift.Client.pdb"), @"DarkRift\Plugins\Client\DarkRift.Client.pdb");
        }
        else
        {
            success[11] = true;
            success[12] = true;
            success[13] = true;
        }

        AssetDatabase.Refresh();

        if (success.All(x => x))
        {
            Debug.Log("All files for version " + GetExpectedAssemblyVersion() + " " + configuration + " were sucessfully imported.");
        }
        else
        {
            string message = success.Count(x => !x) + " failures:\n";
            message += success[0] ? "" : "DarkRift.dll\n";
            message += success[1] ? "" : "DarkRift.xml\n";
            message += success[2] ? "" : "DarkRift.Server.dll\n";
            message += success[3] ? "" : "DarkRift.Server.xml\n";
            message += success[4] ? "" : "DarkRift.Client.dll\n";
            message += success[5] ? "" : "DarkRift.Client.xml\n";
            message += success[6] ? "" : "DarkRift Server (.NET Framework 4.0).zip\n";
            message += success[7] ? "" : "DarkRift Server (.NET Core 2.0).zip\n";
            message += success[8] ? "" : "DarkRift Server (.NET Core 3.1).zip\n";
            message += success[9] ? "" : "DarkRift Server (.NET 5.0).zip\n";
            message += success[10] ? "" : "DarkRift Source.zip\n";
            message += success[11] ? "" : "DarkRift.pdb\n";
            message += success[12] ? "" : "DarkRift.Server.pdb\n";
            message += success[13] ? "" : "DarkRift.Client.pdb\n";

            EditorUtility.DisplayDialog("DarkRift Import Failed", message, "OK");
        }
    }

    /// <summary>
    ///     Removes all imported files.
    /// </summary>
    private static void Clean()
    {
        RemoveFile(@"DarkRift\Plugins\DarkRift.dll");
        RemoveFile(@"DarkRift\Plugins\DarkRift.xml");
        RemoveFile(@"DarkRift\Plugins\DarkRift.pdb");
        RemoveFile(@"DarkRift\Plugins\DarkRift.dll.mdb");

        RemoveFile(@"DarkRift\Plugins\Server\DarkRift.Server.dll");
        RemoveFile(@"DarkRift\Plugins\Server\DarkRift.Server.xml");
        RemoveFile(@"DarkRift\Plugins\Server\DarkRift.Server.pdb");
        RemoveFile(@"DarkRift\Plugins\Server\DarkRift.Server.dll.mdb");

        RemoveFile(@"DarkRift\Plugins\Client\DarkRift.Client.dll");
        RemoveFile(@"DarkRift\Plugins\Client\DarkRift.Client.xml");
        RemoveFile(@"DarkRift\Plugins\Client\DarkRift.Client.pdb");
        RemoveFile(@"DarkRift\Plugins\Client\DarkRift.Client.dll.mdb");

        RemoveFile(@"DarkRift Server (.NET Framework 4.0).zip");
        RemoveFile(@"DarkRift Server (.NET Core 2.0).zip");
        RemoveFile(@"DarkRift Server (.NET Core 3.1).zip");
        RemoveFile(@"DarkRift Server (.NET 5.0).zip");
        RemoveFile(@"DarkRift Source.zip");
    }

    /// <summary>
    ///     Copies a file into Unity.
    /// </summary>
    /// <param name="assembly">The source assembly to take the file from.</param>
    /// <param name="configuration">The configuration being loaded.</param>
    /// <param name="destination">The destination location from Assets\DarkRift\Plugins\</param>
    /// <param name="file">The file name to copy.</param>
    /// <returns>Whether the copy operation succeeded.</returns>
    private static bool CopyFile(string source, string destination)
    {
        string destinationLocation = Path.Combine(@"Assets\DarkRift", destination);

        try
        {
            File.Copy(source, destinationLocation, true);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e.ToString());
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Copies a file into Unity and verifies the assembly version.
    /// </summary>
    /// <param name="assembly">The source assembly to take the file from.</param>
    /// <param name="configuration">The configuration being loaded.</param>
    /// <param name="destination">The destination location from Assets\DarkRift\Plugins\</param>
    /// <param name="file">The file name to copy.</param>
    /// <returns>Whether the copy and verify operation succeeded.</returns>
    private static bool CopyAndVerifyFile(string source, string destination)
    {
        if (!CopyFile(source, destination))
            return false;

        string destinationLocation = Path.Combine(@"Assets\DarkRift", destination);

        Version expectedAssemblyVersion = GetExpectedAssemblyVersion();
        Version actualAssemblyVersionWithRevision = AssemblyName.GetAssemblyName(destinationLocation).Version;
        Version actualAssemblyVersion = new Version(actualAssemblyVersionWithRevision.Major, actualAssemblyVersionWithRevision.Minor, actualAssemblyVersionWithRevision.Build);

        if (actualAssemblyVersion != expectedAssemblyVersion)
        {
            Debug.LogError("Expected assembly version " + expectedAssemblyVersion + " but assembly " + destination + " has version " + actualAssemblyVersion + ".");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Removes a file from the project.
    /// </summary>
    /// <param name="file">The file to remove.</param>
    /// <returns>Whether the remove was successfull.</returns>
    private static bool RemoveFile(string file)
    {
        string fileLocation = Path.Combine(@"Assets\DarkRift", file);

        try
        {
            File.Delete(fileLocation);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e.ToString());
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Compiles a path to the build of a specified folder.
    /// </summary>
    /// <param name="file">The file to get.</param>
    /// <returns>The path compiled.</returns>
    private static string CompilePath(string file)
    {
        string sourceLocation = Path.Combine(ROOT_DIR, "Build");
        sourceLocation = Path.Combine(sourceLocation, file);
        return sourceLocation;
    }

    /// <summary>
    ///     Compiles a path to the build of a specified folder.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="framework">The framework to use.</param>
    /// <param name="file">The file to get.</param>
    /// <returns>The path compiled.</returns>
    private static string CompilePath(string configuration, string framework, string file)
    {
        string sourceLocation = Path.Combine(ROOT_DIR, "Build");
        sourceLocation = Path.Combine(sourceLocation, configuration);
        sourceLocation = Path.Combine(sourceLocation, framework);
        sourceLocation = Path.Combine(sourceLocation, file);
        return sourceLocation;
    }

    /// <summary>
    ///     Checks if a configuration has been built yet.
    /// </summary>
    /// <param name="configuration">The configuration to check.</param>
    /// <returns>If the configuratuion exists.</returns>
    private static bool ConfigurationExists(string configuration)
    {
        string sourceLocation = Path.Combine(ROOT_DIR, "Build");
        sourceLocation = Path.Combine(sourceLocation, configuration);
        return Directory.Exists(sourceLocation);
    }

    /// <summary>
    /// Returns the assembly version in the .props file.
    /// </summary>
    /// <returns></returns>
    private static Version GetExpectedAssemblyVersion()
    {
        return new Version(XDocument.Load(Path.Combine(ROOT_DIR, ".props").ToString()).Root.Element("PropertyGroup").Element("Version").Value);
    }
}
