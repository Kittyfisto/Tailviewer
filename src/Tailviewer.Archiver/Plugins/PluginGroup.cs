using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     Represents all installed versions of a particular plugin (that is all plugin archives with the same plugin id).
	///     Responsible (in the case there are multiple versions) for deciding which particular version of a plugin is actually
	///     loaded.
	/// </summary>
	public sealed class PluginGroup
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		sealed class Plugin
		{
			public IPluginArchive Archive { get; }
			public string FilePath { get; }

			public Plugin(IPluginArchive archive, string filePath)
			{
				Archive = archive;
				FilePath = filePath;
			}
		}

		private readonly Dictionary<Version, Plugin> _pluginsByVersion;
		private readonly PluginId _id;

		private readonly PluginStatus _status;
		private PluginDescription _selectedPlugin;
		private IPluginArchive _selectedPluginArchive;

		public PluginGroup(PluginId id)
		{
			_id = id;
			_pluginsByVersion = new Dictionary<Version, Plugin>();
			_status = new PluginStatus
			{
				IsInstalled = false,
				IsLoaded = false
			};
		}

		public IPluginStatus Status
		{
			get { return _status; }
		}

		public IPluginDescription Description
		{
			get { return _selectedPlugin; }
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var plugin in _pluginsByVersion.Values) plugin.Archive.Dispose();
		}

		#endregion

		public void Add(string pluginPath, Version pluginVersion, IPluginArchive archive)
		{
			if (_pluginsByVersion.TryGetValue(pluginVersion, out var existingPlugin))
			{
				Log.WarnFormat("Ignoring plugin '{0}', an identical version has already been loaded from '{1}'!",
				               pluginPath,
				               existingPlugin.FilePath);
				return;
			}

			_pluginsByVersion.Add(pluginVersion, new Plugin(archive, pluginPath));
			_status.IsInstalled = true;
		}

		/// <summary>
		///     Loads the best version of the given plugin.
		/// </summary>
		public void Load()
		{
			Log.DebugFormat("Found {0} version(s) of plugin '{1}', finding compatible ones...", _pluginsByVersion.Count, _id);

			var compatiblePlugins = _pluginsByVersion.Where(x => !(x.Value.Archive is EmptyPluginArchive))
			                                          .Where(x => IsCompatible(x.Value.Archive.Index))
			                                          .OrderByDescending(x => x.Key)
			                                          .Select(x => x.Value).ToList();

			if (compatiblePlugins.Any())
			{
				Log.DebugFormat("Found {0} compatible version(s) of plugin '{1}', loading newest...", _pluginsByVersion.Count, _id);

				// It's possible that despite our best efforts, the plugin with the highest version
				// refuses to be loaded (for example because it's broken, or the compatibility check might miss something).
				// If that's the case, we try to load earlier versions until we either find one that works or
				// give up...
				if (TryLoad(compatiblePlugins, out var loadedPlugin))
				{
					_selectedPlugin = CreateDescription(loadedPlugin);
					_selectedPluginArchive = loadedPlugin.Archive;
					_status.IsLoaded = true;
				}
				else
				{
					_selectedPlugin = new PluginDescription
					{
						Id = _id,
						Error = "The plugin couldn't be loaded",
						PluginImplementations = new IPluginImplementationDescription[0]
					};
					var plugin = compatiblePlugins.First(); //< Never empty: we have at least 1 plugin
					_selectedPlugin.FilePath = plugin.FilePath;
					_selectedPlugin.Version = plugin.Archive.Index.Version;

					_status.IsLoaded = false;
				}
			}
			else
			{
				Log.ErrorFormat("Found 0 compatible version(s) of plugin '{0}' (tried all {1} version(s) of this plugin)", _id, _pluginsByVersion.Count);

				_selectedPlugin = new PluginDescription
				{
					Id = _id,
					Error = "The plugin couldn\'t be loaded",
					PluginImplementations = new IPluginImplementationDescription[0]
				};

				if (_pluginsByVersion.Any()) //< Might be empty
				{
					var plugin = _pluginsByVersion.Values.First();
					_selectedPlugin.FilePath = plugin.FilePath;
					_selectedPlugin.Version = plugin.Archive.Index.Version;
				}

				_status.IsLoaded = false;
			}
		}

		public IEnumerable<PluginWithDescription<T>> LoadAllOfType<T>() where T : class, IPlugin
		{
			return LoadAllOfType(typeof(T)).Select(pair => new PluginWithDescription<T>((T) pair.Plugin, pair.Description));
		}

		public IReadOnlyList<PluginWithDescription<IPlugin>> LoadAllOfType(Type pluginType)
		{
			var preferredPlugin = _selectedPlugin;
			if (preferredPlugin == null)
				return new List<PluginWithDescription<IPlugin>>();

			var types = new List<PluginWithDescription<IPlugin>>();
			foreach (var implementation in preferredPlugin.PluginImplementations)
			{
				if (implementation.InterfaceType == pluginType)
				{
					try
					{
						Log.DebugFormat("Creating instance of plugin '{0}' from '{1}'...", implementation.FullTypeName,
						               preferredPlugin.FilePath);

						var assembly = _selectedPluginArchive.LoadPlugin(); //< is cached so multiple calls are fine
						var type = assembly.GetType(implementation.FullTypeName);
						var pluginObject = Activator.CreateInstance(type);
						types.Add(new PluginWithDescription<IPlugin>((IPlugin) pluginObject, preferredPlugin));

						Log.InfoFormat("Created instance of plugin '{0}' from '{1}'", implementation.FullTypeName,
						               preferredPlugin.FilePath);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Unable to load plugin of interface '{0}' from '{1}': {2}",
						                pluginType.FullName,
						                preferredPlugin,
						                e);
					}
				}
			}

			return types;
		}

		/// <summary>
		/// Only used in conjunction with 
		/// </summary>
		/// <returns></returns>
		public bool TryLoadAllTypes()
		{
			var assembly = _selectedPluginArchive.LoadPlugin(); //< is cached so multiple calls are fine
			try
			{
				var types = assembly.GetExportedTypes();
				foreach (var type in types)
				{
					try
					{
						Activator.CreateInstance(type);
						Log.InfoFormat("Created instance of '{0}'", type);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Unable to load '{0}':\r\n{1}", type, e);
						return false;
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught exception while trying to load all exported types:\r\n{0}", e);
				return false;
			}

			return true;
		}

		/// <summary>
		///     Tests if a plugin with the given index is actually usable (or if the plugin is too old, or too new).
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		public static bool IsCompatible(IPluginPackageIndex index)
		{
			return !FindCompatibilityErrors(index).Any();
		}

		[Pure]
		public static List<PluginError> FindCompatibilityErrors(IPluginPackageIndex index)
		{
			var errors = new List<PluginError>();
			if (index.PluginArchiveVersion < PluginArchive.MinimumSupportedPluginArchiveVersion)
				errors.Add(new
					           PluginError("The plugin targets an older version of Tailviewer and must be compiled against the current version in order to be usable"));

			if (index.PluginArchiveVersion > PluginArchive.CurrentPluginArchiveVersion)
				errors.Add(new
					           PluginError("The plugin targets a newer version of Tailviewer and must be compiled against the current version in order to be usable"));

			if (index.ImplementedPluginInterfaces != null)
				foreach (var implementation in index.ImplementedPluginInterfaces)
					if (TryResolvePluginInterface(implementation, out var @interface))
					{
						var currentInterfaceVersion = PluginInterfaceVersionAttribute.GetInterfaceVersion(@interface);
						var implementedInterfaceVersion = new PluginInterfaceVersion(implementation.InterfaceVersion);

						if (implementedInterfaceVersion < currentInterfaceVersion)
							errors.Add(new
								           PluginError($"The plugin implements an older version of '{@interface.FullName}'. It must target the current version in order to be usable!"));
						else if (implementedInterfaceVersion > currentInterfaceVersion)
							errors.Add(new
								           PluginError($"The plugin implements a newer version of '{@interface.FullName}'. It must target the current version in order to be usable!"));
					}
					else
					{
						// If a plugin unfortunately implements an interface that is not known to this tailviewer,
						// then it is bad news because that means we won't be able to load its assembly!
						errors.Add(new
							           PluginError($"The plugin implements an unknown interface '{implementation.InterfaceTypename}' which is probably part of a newer tailviewer version. The plugin should target the current version in order to be usable!"));
					}

			return errors;
		}

		public IReadOnlyDictionary<string, Type> ResolveSerializableTypes()
		{
			var serializableTypes = new Dictionary<string, Type>();

			var archive = _selectedPluginArchive;
			if (archive != null)
			{
				var assembly = archive.LoadPlugin();
				var types = _selectedPlugin.SerializableTypes;
				foreach (var tmp in types)
					if (TryResolveType(assembly, tmp.Value, out var type))
						serializableTypes.Add(tmp.Key, type);
			}

			return serializableTypes;
		}

		/// <summary>
		/// Tries to load the plugins contained in the given archives.
		/// Returns once the first plugin was successfully loaded.
		/// </summary>
		/// <param name="plugins"></param>
		/// <param name="loaded"></param>
		/// <returns></returns>
		private bool TryLoad(IEnumerable<Plugin> plugins, out Plugin loaded)
		{
			foreach (var plugin in plugins)
			{
				if (TryLoad(plugin))
				{
					loaded = plugin;
					return true;
				}
			}

			loaded = null;
			return false;
		}

		/// <summary>
		/// Tries to load the plugin contained in the given archive.
		/// </summary>
		/// <param name="plugin"></param>
		/// <returns></returns>
		private bool TryLoad(Plugin plugin)
		{
			Log.DebugFormat("Trying to load plugin '{0}'...", plugin.FilePath);

			try
			{
				plugin.Archive.LoadPlugin();
				Log.InfoFormat("Loaded plugin '{0}'", plugin.FilePath);
				return true;
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to load plugin '{0}':\r\n{1}",
				               plugin.FilePath,
				               e);
				return false;
			}
		}

		private static bool TryResolvePluginInterface(PluginInterfaceImplementation description, out Type interfaceType)
		{
			try
			{
				// GetType(..., false) does not cut it because that STILL throws exceptions, yay!
				interfaceType = ResolvePluginInterface(description);
				return interfaceType != null;
			}
			catch (Exception e)
			{
				Log.DebugFormat("Caught exception while trying to resolve interface '{0}':\r\n{1}",
				                description.InterfaceTypename,
				                e);
				interfaceType = null;
				return false;
			}
		}

		private bool TryResolveType(Assembly assembly, string typeName, out Type type)
		{
			try
			{
				type = assembly.GetType(typeName, throwOnError: true);
				if (type == null)
				{
					Log.ErrorFormat("Unable to resolve type '{0}'", typeName);
					return false;
				}

				return true;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to resolve type '{0}': {1}", typeName, e);
				type = null;
				return false;
			}
		}

		[Pure]
		private static PluginDescription CreateDescription(Plugin plugin)
		{
			var archiveIndex = plugin.Archive.Index;

			Uri.TryCreate(archiveIndex.Website, UriKind.Absolute, out var website);

			var plugins = new List<IPluginImplementationDescription>();
			foreach (var description in archiveIndex.ImplementedPluginInterfaces)
			{
				var pluginInterfaceType = ResolvePluginInterface(description);
				if (pluginInterfaceType != null)
					plugins.Add(new PluginImplementationDescription(description)
					{
						InterfaceType = pluginInterfaceType
					});
				else
					Log.WarnFormat("Plugin implements unknown interface '{0}', skipping it...",
					               description.InterfaceTypename);
			}

			var serializableTypes = new Dictionary<string, string>();
			foreach (var pair in archiveIndex.SerializableTypes) serializableTypes.Add(pair.Name, pair.FullName);

			var changes = new List<Change>(archiveIndex.Changes.Count);
			foreach(var serializableChange in archiveIndex.Changes)
				changes.Add(new Change(serializableChange));

			var desc = new PluginDescription
			{
				Id = new PluginId(archiveIndex.Id),
				Name = archiveIndex.Name,
				Version = archiveIndex.Version,
				Icon = LoadIcon(plugin.Archive.ReadIcon()),
				FilePath = plugin.FilePath,
				Author = archiveIndex.Author,
				Description = archiveIndex.Description,
				Website = website,
				PluginImplementations = plugins,
				SerializableTypes = serializableTypes,
				Changes = changes
			};

			return desc;
		}

		private static Type ResolvePluginInterface(PluginInterfaceImplementation description)
		{
			return typeof(IPlugin).Assembly.GetType(description.InterfaceTypename);
		}

		private static ImageSource LoadIcon(Stream icon)
		{
			if (icon == null)
				return null;

			var image = new BitmapImage();
			image.BeginInit();
			image.StreamSource = icon;
			image.EndInit();
			return image;
		}
	}
}