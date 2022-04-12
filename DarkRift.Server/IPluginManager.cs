/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server
{
    /// <summary>
    ///     Interface for the plugin manager that handles plugins.
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        ///     Gets the plugin with the specified name.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <returns>The plugin.</returns>
        /// <remarks>
        ///     O(1) complexity.
        ///     This cannot be called during server initialization as not all plugins may 
        ///     have been loaded at that point, consider using the 
        ///     <see cref="ExtendedPluginBase.Loaded(LoadedEventArgs)"/> event instead.
        /// </remarks>
        Plugin this[string name] { get; }

#if PRO
        /// <summary>
        ///     Gets all the plugins loaded into this server including internal ones.
        /// </summary>
        /// <returns>An array of the plugins.</returns>
        /// <remarks>
        ///     Pro only.
        ///     
        ///     This cannot be called during server initialization as not all plugins may 
        ///     have been loaded at that point, consider using the 
        ///     <see cref="ExtendedPluginBase.Loaded(LoadedEventArgs)"/> event instead.
        /// </remarks>

        Plugin[] ActuallyGetAllPlugins();

        /// <summary>
        ///     Gets all the plugins loaded into this server.
        /// </summary>
        /// <returns>An array of the plugins.</returns>
        /// <remarks>
        ///     Pro only.
        ///
        ///     This cannot be called during server initialization as not all plugins may 
        ///     have been loaded at that point, consider using the 
        ///     <see cref="ExtendedPluginBase.Loaded(LoadedEventArgs)"/> event instead.
        /// </remarks>
        Plugin[] GetAllPlugins();

        /// <summary>
        ///     Retrieves the current version of a plugin installed.
        /// </summary>
        /// <param name="pluginName">The name of the plugin to look up.</param>
        /// <returns>The version of the plugin or null if not installed.</returns>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        Version GetInstalledVersion(string pluginName);
#endif

        /// <summary>
        ///     Gets the plugin with the specified name.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <returns>The plugin.</returns>
        /// <remarks>
        ///     O(1) complexity.
        ///     
        ///     This cannot be called during server initialization as not all plugins may 
        ///     have been loaded at that point, consider using the 
        ///     <see cref="ExtendedPluginBase.Loaded(LoadedEventArgs)"/> event instead.
        /// </remarks>
        Plugin GetPluginByName(string name);

        /// <summary>
        ///     Gets the plugin of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the plugin to load.</typeparam>
        /// <returns>The plugin.</returns>
        /// <remarks>
        ///     O(n) complexity.
        ///     
        ///     This cannot be called during server initialization as not all plugins may 
        ///     have been loaded at that point, consider using the 
        ///     <see cref="ExtendedPluginBase.Loaded(LoadedEventArgs)"/> event instead.
        /// </remarks>
        T GetPluginByType<T>() where T : Plugin;
    }
}
