/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DarkRift.Server
{
    /// <summary>
    ///     Factory for creating plugins of various types.
    /// </summary>
    internal sealed class PluginFactory
    {
        /// <summary>
        ///     The list of types that can be loaded.
        /// </summary>
        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        /// <summary>
        ///     The logger this factory will use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Creates a new PluginFactory.
        /// </summary>
        /// <param name="logger">The logger this factory will use.</param>
        internal PluginFactory(Logger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        ///     Adds plugins based on the plugins settings supplied.
        /// </summary>
        /// <param name="settings">The settings defining where to find plugins.</param>
        internal void AddFromSettings(ServerSpawnData.PluginSearchSettings settings)
        {
            AddTypes(settings.PluginTypes);

            foreach (ServerSpawnData.PluginSearchSettings.PluginSearchPath path in settings.PluginSearchPaths)
            {
                if (File.Exists(path.Source))
                {
                    if (path.DependencyResolutionStrategy == DependencyResolutionStrategy.RecursiveFromDirectory)
                        throw new InvalidOperationException($"{nameof(DependencyResolutionStrategy)}.{nameof(DependencyResolutionStrategy.RecursiveFromDirectory)} cannot be used with path to a file.");
                    AddFile(path.Source, path.DependencyResolutionStrategy, null);
                }
                else
                {
                    AddDirectory(path.Source, path.CreateDirectory, path.DependencyResolutionStrategy);
                }
            }
        }

        /// <summary>
        ///     Adds a directory of plugin files to the index.
        /// </summary>
        /// <param name="directory">The directory to add.</param>
        /// <param name="create">Whether to create the directory if not present.</param>
        /// <param name="dependencyResolutionStrategy">The way to resolve dependencies for the plugin.</param>
        internal void AddDirectory(string directory, bool create, DependencyResolutionStrategy dependencyResolutionStrategy)
        {
            //Create plugin directory if not present
            if (Directory.Exists(directory))
            {
                //Get the names of all files to try and load
                string[] pluginSourceFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);

                AddFiles(pluginSourceFiles, dependencyResolutionStrategy, directory);
            }
            else
            {
                if (create)
                    Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        ///     Adds the given plugin files into the index.
        /// </summary>
        /// <param name="files">An array of filepaths to the plugins.</param>
        /// <param name="dependencyResolutionStrategy">The way to resolve dependencies for the plugin.</param>
        /// <param name="searchedDirectory">the directory that was searched to find this file.</param>
        internal void AddFiles(IEnumerable<string> files, DependencyResolutionStrategy dependencyResolutionStrategy, string searchedDirectory)
        {
            //Load each file to a plugin
            foreach (string pluginSourceFile in files)
                AddFile(pluginSourceFile, dependencyResolutionStrategy, searchedDirectory);
        }

        /// <summary>
        ///     Adds plugins into the server from the given types.
        /// </summary>
        /// <param name="pluginTypes">The types of plugins to add.</param>
        internal void AddTypes(IEnumerable<Type> pluginTypes)
        {
            foreach (Type pluginType in pluginTypes)
                AddType(pluginType);
        }

        /// <summary>
        ///     Adds all plugin types in the file to the index.
        /// </summary>
        /// <param name="file">The file containing the types.</param>
        /// <param name="dependencyResolutionStrategy">The way to resolve dependencies for the plugin.</param>
        /// <param name="searchedDirectory">the directory that was searched to find this file.</param>
        internal void AddFile(string file, DependencyResolutionStrategy dependencyResolutionStrategy, string searchedDirectory)
        {
            //Check the file is a dll
            if (Path.GetExtension(file) != ".dll")
                throw new ArgumentException("The filepath supplied was not a DLL library.");

            //Check the file exists
            if (!File.Exists(file))
                throw new FileNotFoundException("The specified filepath does not exist.");

            //Log
            logger.Trace($"Searching '{file}' for plugins.");

            // Setup assembly resolver to help find dependencies recursively in the folder heirachy of the plugin
            AppDomain.CurrentDomain.AssemblyResolve += LoadFromSameFolder;

            Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
            {
                string rootFolderPath;
                if (dependencyResolutionStrategy == DependencyResolutionStrategy.RecursiveFromFile)
                {
                    rootFolderPath = Path.GetDirectoryName(file);
                }
                else if (searchedDirectory != null)
                {
                    rootFolderPath = searchedDirectory;
                }
                else
                {
                    return null;
                }

                string assemblyPath = SearchForFile(rootFolderPath, new AssemblyName(args.Name).Name + ".dll");
                if (assemblyPath == null)
                    return null;

                return Assembly.LoadFrom(assemblyPath);
            }

            //Load the assembly
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(Path.GetFullPath(file));
            }
            catch (Exception e)
            {
                logger.Error($"{file} could not be loaded as an exception occurred.", e);
                return;
            }

            //Get the types in the assembly
            IEnumerable<Type> enclosedTypes;
            try
            {
                enclosedTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                logger.Error(
                    $"Failed to load one or more plugins from DLL file '{file}', see following logs for more info.\n\nIf this file is a DarkRift plugin rebuilding this plugin may help. Make sure it is built against the same .NET target as DarkRift is running and built to a compatible version ({Environment.Version}).\n\nThis exception can also occur when an unmanaged DLL is loaded by DarkRift because it is in a plugin search path. If this is the case, consider moving the unmanaged library out of any plugin search paths (https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/default-probing#unmanaged-native-library-probing) or modify the plugin search path configuration to avoid discovering the library (e.g. reference individual DLL files instead of the containing directory).",
                    e
                );

                foreach (Exception loaderException in e.LoaderExceptions)
                {
                    logger.Error("Additional exception detail from LoaderExceptions property:", loaderException);
                }

                // The types unable to be loaded will be null here!
                enclosedTypes = e.Types.Where(t => t != null);
            }

            //Find the types that are plugins
            foreach (Type enclosedType in enclosedTypes)
            {
                if (enclosedType.IsSubclassOf(typeof(PluginBase)) && !enclosedType.IsAbstract)
                {
                    //Add the plugin
                    AddType(enclosedType);
                }
            }

            // Remove resolver again
            AppDomain.CurrentDomain.AssemblyResolve -= LoadFromSameFolder;
        }

        /// <summary>
        ///     Adds a type to the lookup.
        /// </summary>
        /// <param name="plugin">The plugin type to add.</param>
        internal void AddType(Type plugin)
        {
            if (!plugin.IsSubclassOf(typeof(PluginBase)))
                throw new ArgumentException($"The type supplied, '{plugin.Name}', was not a plugin. Ensure the type inherits from PluginBase.");

            if (plugin.IsAbstract)
                throw new ArgumentException($"The type supplied, '{plugin.Name}', was marked as abstract. Ensure the type is not abstract.");

            // Add if it has not already been added (and warn if two different types with the same name are added)
            if (!types.ContainsKey(plugin.Name))
                types.Add(plugin.Name, plugin);
            else if (types[plugin.Name] != plugin)
                logger.Error($"A plugin '{plugin.Name}' could not be added to the plugin factory as it was already present. This is likely because two plugins have the same name or the same DLL has been loaded multiple times. Consider renaming your plugins to avoid conflicts (note, namespaces are not considered) or update the plugin search paths configuration to avoid loading the same DLL multiple times.");
        }

        /// <summary>
        ///     Creates a named type as a specified plugin.
        /// </summary>
        /// <typeparam name="T">The type of plugin to load it as.</typeparam>
        /// <param name="type">The name of the type to load.</param>
        /// <param name="loadData">The data to load into the plugin.</param>
        /// <param name="backupLoadData">The backup load data to try for backwards compatablity.</param>
        /// <returns>The new plugin.</returns>
        internal T Create<T>(string type, PluginBaseLoadData loadData, PluginLoadData backupLoadData = null) where T : PluginBase
        {
            try
            {
                return Create<T>(types[type], loadData, backupLoadData);
            }
            catch (KeyNotFoundException e)
            {
                throw new KeyNotFoundException($"No plugin of type {type} is available to load. Check the plugin is in the plugin search paths, you can see the plugins that are found by setting startup log levels to show trace level logs.", e);
            }
        }

        /// <summary>
        ///     Creates a type as a specified plugin.
        /// </summary>
        /// <typeparam name="T">The type of plugin to load it as.</typeparam>
        /// <param name="type">The type to load.</param>
        /// <param name="loadData">The data to load into the plugin.</param>
        /// <param name="backupLoadData">The backup load data to try for backwards compatability.</param>
        /// <returns>The new plugin.</returns>
        internal T Create<T>(Type type, PluginBaseLoadData loadData, PluginLoadData backupLoadData = null) where T : PluginBase
        {
            //Create an instance of the plugin
            T plugin;
            try
            {
                plugin = (T)Activator.CreateInstance(type, loadData);
            }
            catch (MissingMethodException)
            {
                //Failed, perhaps using backup PluginLoadData would help?
                if (backupLoadData != null)
                {
                    plugin = (T)Activator.CreateInstance(type, backupLoadData);
                    logger.Warning($"Plugin {type.Name} was loaded using a PluginLoadData object instead of the desired {loadData.GetType().Name}. Consider replacing the constructor with one accepting a " + loadData.GetType().Name + " object instead.");
                }
                else
                {
                    throw;
                }
            }

            //Log creation
            if (!plugin.Hidden)
                logger.Trace($"Created plugin '{type.Name}'.");
            
            return plugin;
        }

        /// <summary>
        ///     Returns a list of plugins found that are subtypes of that given.
        /// </summary>
        /// <param name="type">The type to filter by.</param>
        /// <returns>The types found.</returns>
        internal Type[] GetAllSubtypes(Type type)
        {
            return types.Values
                .Where(t => t.IsSubclassOf(type))
                .ToArray();
        }

        private string SearchForFile(string rootDirectory, string fileName)
        {
            // Try and find the file in this directory
            string filePath = Path.Combine(rootDirectory, fileName);
            if (File.Exists(filePath))
                return filePath;

            // If it's not there look in the subdirectories
            foreach (string subDirectory in Directory.GetDirectories(rootDirectory))
            {
                string found = SearchForFile(subDirectory, fileName);
                if (found != null)
                    return found;
            }

            // Otherwise it doesn't exist
            return null;
        }
    }
}
