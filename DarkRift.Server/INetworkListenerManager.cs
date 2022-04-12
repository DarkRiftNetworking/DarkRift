/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
    /// <summary>
    ///     Interface for the listener manager that handles network listeners.
    /// </summary>
    public interface INetworkListenerManager
    {
        /// <summary>
        ///     Gets the listener with the specified name.
        /// </summary>
        /// <param name="name">The name of the listener.</param>
        /// <returns>The listener.</returns>
        /// <remarks>
        ///     O(1) complexity.
        ///     This cannot be called during server initialization as not all plugins may 
        ///     have been loaded at that point, consider using the 
        ///     <see cref="ExtendedPluginBase.Loaded(LoadedEventArgs)"/> event instead.
        /// </remarks>
        NetworkListener this[string name] { get; }

        /// <summary>
        ///     Gets the listeners loaded into this server.
        /// </summary>
        /// <returns>An array of the listeners.</returns>
        NetworkListener[] GetNetworkListeners();

#if PRO
        /// <summary>
        ///     Gets all the listeners loaded into this server.
        /// </summary>
        /// <returns>An array of the listeners.</returns>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        NetworkListener[] GetAllNetworkListeners();
#endif

        /// <summary>
        ///     Gets the listener with the specified name.
        /// </summary>
        /// <param name="name">The name of the listener.</param>
        /// <returns>The listener.</returns>
        /// <remarks>
        ///     O(1) complexity.
        /// </remarks>
        NetworkListener GetNetworkListenerByName(string name);

        /// <summary>
        ///     Gets the listeners of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the listener to load.</typeparam>
        /// <returns>The listeners.</returns>
        /// <remarks>
        ///     O(n) complexity.
        /// </remarks>
        T[] GetNetworkListenersByType<T>() where T : NetworkListener;
    }
}
