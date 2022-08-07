DarkRift is build using [Cake](https://cakebuild.net/). To create a full build of DarkRift run `dotnet cake` (you will need to run `dotnet tool restore` the first time).

This command will:
- Build DarkRift
- Create neatly organized packages in `./Build`
- Test DR

You will need the .NET 6 SDK to build DarkRift.

For editing DarkRift we recommend using Microsoft Visual Studio 2022 although other editors will work.

## Cake Tasks
The following tasks are defined in `build.cake`:
- `Clean` - Cleans the `./Build` directory
- `Build` - Builds DarkRift
- `Test` (default) - Builds and tests DarkRift
- `InstallInCLITool` - Builds and installs a dev version of DarkRift into the DarkRift CLI tool as version `dev`. This is useful for easy local testing

The following arguments can be passed to the build script:
- `--target <task>` - choose the task to run (see list above)
- `--configuration <configuration>` - the configuration of DR to build (see below section)
- `--core-only` - only build for .NET Core, this can get around issues building .NET Framework applications such as on Linux

For example:
```bash
dotnet cake --target InstallInCLITool --configuration Pro
```

## Configurations

Currently, DarkRift can be built in the following configurations:
* Debug: Self evident.
* Free: The same as the old Free version.
* Pro: The same as the old Pro version. This is probably want you want for release builds. Going forward, it is also what will be supported in future DR2 versions.

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
