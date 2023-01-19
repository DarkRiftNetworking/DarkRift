# Building DarkRift from Source

To build the DarkRift 2 solution *you must have Microsoft Visual Studio 2022* installed. The Community edition is fine for open source projects, and is easily attained from https://visualstudio.microsoft.com/vs/community/

If you get an error when building like "The targeting pack Microsoft.NETCore.App is not installed. Please restore and try again.", consider installing the latest .NET SDK, at time of writing https://dotnet.microsoft.com/en-us/download/dotnet/7.0. Note that Microsoft dropped "Core" and now just calls it .NET (again).

## Configurations

Currently, DarkRift can be built in the following configurations:
* Debug.
* Release.

The old distinction between Free and Pro versions are gone and have been merged into these configurations!

## Output Assemblies

Builds emit to the ./Build subdirectory. A subfolder is created for each configuration, which in turn has a subfolder for each output platform (e.g. .NET 4.0).

In each platform subfolder, there is:
* A Lib subfolder containing assembles, debug symbol .pdb files, etc.
* A Run command to launch the standalone DarkRift server.
* A default Server.config.
* DarkRift Server.zip containing the files in the platform folder.

Additionally, there is a ./Build/DarkRift Source.zip containing a cleaned copy of the project, which corresponds to the old way the DR2 source code used to be distributed with the Pro version. It is recommended to simply refer users to this GitHub repo instead.

If you're looking to manually copy assemblies into your Unity assets, you should look for .dll files from the ./Build/($configuration)/net4.0/Lib subfolder. Remember to paste assemblies into a Plugins folder or subfolder in order for Unity to recognize them.

## Producing a .unitypackage

If you're an existing DarkRift 2 user, there's a decent chance you will want to reproduce the Free or Pro .unitypackage DarkRift 2 used to ship with in order to stick with the way DR2 in your Assets folder is organized. Even as a new user, a .unitypackage can be a practical way install the typical DarkRift version into your Unity project if you don't intend to make changes.

*TODO: Fix package building in post_release.sh and write guide for how to export packages*