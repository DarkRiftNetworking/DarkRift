/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
    /// <summary>
    /// The way that dependencies of a plugin will be found.
    /// </summary>
    public enum DependencyResolutionStrategy
    {
        /// <summary>
        /// Uses the standard .NET assembly resolution with no enhancements.
        /// </summary>
        /// <remarks>
        /// This strategy will search for dependencies for using the standard .NET rules detailed <see cref="!:https://docs.microsoft.com/en-us/dotnet/framework/deployment/how-the-runtime-locates-assemblies">here</see>.
        /// </remarks>
        Standard,

        /// <summary>
        /// Recursively searches all subdirectories of each file's containing folder in addition to the standard .NET assembly resolution.
        /// </summary>
        /// <remarks>
        /// This strategy will search for dependencies for using the standard .NET rules detailed <see cref="!:https://docs.microsoft.com/en-us/dotnet/framework/deployment/how-the-runtime-locates-assemblies">here</see> but with additional steps to search through each discovered file's containing folder and any subdirectory of it.
        /// </remarks>
        RecursiveFromFile,

        /// <summary>
        /// Recursively searches all subdirectories of the folder's containing folder in addition to the standard .NET assembly resolution.
        /// </summary>
        /// <remarks>
        /// This strategy will search for dependencies for using the standard .NET rules detailed <see cref="!:https://docs.microsoft.com/en-us/dotnet/framework/deployment/how-the-runtime-locates-assemblies">here</see> but with additional steps to search through the specified directory and any subdirectory of it. This cannot be used when using a path to a file.
        /// </remarks>
        RecursiveFromDirectory
    }
}
