var target = Argument("target", "Test");
var configuration = Argument("configuration", "Debug");
var coreOnly = HasArgument("core-only");

var version = System.IO.File.ReadAllText(".version").Trim();

Task("Clean")
    .Does(() =>
{
    Information($"Cleaning ./Build/{configuration}");
    CleanDirectory($"./Build/{configuration}");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./DarkRift2.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = new DotNetMSBuildSettings
        {
            Properties = 
            {
              { "Version", new List<string> {version} },
              { "DRBuildMode", new List<string> {coreOnly ? "coreonly" : "all"} }
            }
        }
    });

    var targetFrameworks = GetSubDirectories($"./DarkRift.Server.Console/bin/{configuration}")
                              .Select(d => MakeRelative(d, MakeAbsolute(Directory($"./DarkRift.Server.Console/bin/{configuration}"))));
    foreach (var targetFramework in targetFrameworks)
    {
        var sourceDir = $"./DarkRift.Server.Console/bin/{configuration}/{targetFramework}";
        var buildDir = $"./Build/{configuration}/{targetFramework}";

        Information($"Constructing {buildDir}");

        CreateDirectory(buildDir);

        // Copy files into Build folder
        CopyFiles($"{sourceDir}/*.*", buildDir);
            
        // .NET Framework needs a slightly different structure
        if (targetFramework == "net4.0")
        {
            // Move library files into the Libs folder (except DarkRift.Server.Console.pdb)
            CreateDirectory($"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.dll", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.pdb", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/Lib/DarkRift.Server.Console.pdb", $"{buildDir}");
            MoveFiles($"{buildDir}/*.xml", $"{buildDir}/Lib");
        }
        else
        {
            // Move library files into the Libs folder
            CreateDirectory($"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.dll", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.pdb", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.xml", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.deps.json", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.dll.config", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.runtimeconfig.dev.json", $"{buildDir}/Lib");
            MoveFiles($"{buildDir}/*.runtimeconfig.json", $"{buildDir}/Lib");
        }

        // Create zip of server build
        // Zip(buildDir, $"{buildDir}/DarkRift Server.zip");
    }
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./DarkRift2.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("InstallInCLITool")
    .IsDependentOn("Build")
    .Does(() =>
{
    var homeDir = IsRunningOnWindows() ? EnvironmentVariable("HOMEDRIVE") + EnvironmentVariable("HOMEPATH") : EnvironmentVariable("HOME");
    var tier = configuration == "Debug" ? "pro" : configuration.ToLower();  // CLI doesn't support a 'debug' tier so remap to pro

    var targetFrameworks = GetSubDirectories($"./DarkRift.Server.Console/bin/{configuration}")
                              .Select(d => MakeRelative(d, MakeAbsolute(Directory($"./DarkRift.Server.Console/bin/{configuration}"))));
    foreach (var targetFramework in targetFrameworks)
    {
        var buildDir = $"./Build/{configuration}/{targetFramework}";
        var cliDir = $"/{configuration}/{targetFramework}";
        var platform = targetFramework == "net4.0" ? "net40" : targetFramework;     // TODO for some reason .NET 4.0 is under net40 in DR CLI

        Information($"Installing into ~/.darkrift/installed/{tier}/{platform}/dev");

        EnsureDirectoryExists($"{homeDir}/.darkrift/installed/{tier}/{platform}/dev");
        CleanDirectory($"{homeDir}/.darkrift/installed/{tier}/{platform}/dev");
        CopyFiles($"{buildDir}/**/*.*", $"{homeDir}/.darkrift/installed/{tier}/{platform}/dev");
    }
});

// Task("Release")
//     .Does(() =>
// {
// #addin nuget:?package=Cake.Prompt&version=1.2.1
//     var versionRegex = "^[0-9]+.[0-9]+.[0-9]+$";
//     var releaseVersion = version; 
//     do
//       releaseVersion = Prompt("Enter version for this release", version.Replace("-prerelease", "")).Trim();
//     while (!Regex.IsMatch(releaseVersion, versionRegex));
//     version = releaseVersion;
//
//     if (StartProcess("git", new ProcessSettings { Arguments = "diff-index --quiet HEAD --" }) != 0)
//         throw new Exception("You have uncommitted changes in your project. Commit the changes or reset them to release.");
//
//     // TODO Port to C#
//     // Information("DarkRift $version - $(date -I)");
//     // git log $(git describe --tags --abbrev=0)..HEAD --format='- %s' | grep -vF "[changelog.skip]" | grep -v "^Merge"
//
//     RunTarget("Build");
//
//     do
//       version = Prompt($"Enter prerelease version (previous was {version}) (-prerelease will be appended)") + "-prerelease";
//     while (!Regex.IsMatch(version, versionRegex));
//
//     System.IO.File.WriteAllText(".version", version);
//
//     StartProcess("git", new ProcessSettings { Arguments = $"commit -a \"Update version number to {version}\"" });
//     StartProcess("git", new ProcessSettings { Arguments = $"tag V{version}" });
//
//     StartProcess("git", new ProcessSettings { Arguments = "push origin master --tags" });
//
//     RunTarget("CopyToUnity");  // TODO port editor script in Unity to cake
//
// #addin nuget:?package=Cake.Unity&version=0.9.0
//     UnityEditor(
//         new UnityEditorArguments
//         {
//             ProjectPath = "./DarkRift.Unity",
//             ExportPackage = new ExportPackage { AssetPaths = { "Assets/DarkRift" }, PackageName = "DarkRift.unitypackage"}
//         }
//     );
//
// #addin nuget:?package=Cake.DocFx&version=1.0.0
//     DocFxBuild("./DarkRift.Documentation/docfx.json");
// });

// Run a target when script is run
RunTarget(target);
