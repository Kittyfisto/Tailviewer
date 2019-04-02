using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
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
	///     This class is responsible for loading plugin archives which have been created with <see cref="PluginPacker" />
	///     (or the cli equivalent, packer.exe).
	/// </summary>
	public sealed class PluginArchiveLoader
		: IPluginLoader
			, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<IPluginDescription, IPluginArchive> _archivesByPlugin;
		private readonly Dictionary<PluginId, IPluginStatus> _pluginStati;

		/// <summary>
		/// </summary>
		/// <param name="filesystem"></param>
		/// <param name="path"></param>
		public PluginArchiveLoader(IFilesystem filesystem, string path)
		{
			_archivesByPlugin = new Dictionary<IPluginDescription, IPluginArchive>();
			_pluginStati = new Dictionary<PluginId, IPluginStatus>();

			try
			{
				// TODO: How would we make this truly async? Currently the app has to block until all plugins are loaded wich is sad
				var files = filesystem.EnumerateFiles(path, string.Format("*.{0}", PluginArchive.PluginExtension))
				                      .Result;
				foreach (var pluginPath in files)
					ReflectPlugin(pluginPath);
			}
			catch (DirectoryNotFoundException e)
			{
				Log.WarnFormat("Unable to find plugins in '{0}': {1}", path, e);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to find plugins in '{0}': {1}", path, e);
			}
		}

		/// <inheritdoc />
		public IEnumerable<IPluginDescription> Plugins
		{
			get
			{
				var plugins = _archivesByPlugin.GroupBy(x => x.Key.Id).Select(FindUsablePlugin).Where(x => x != null)
				                               .ToList();
				return plugins;
			}
		}

		[Pure]
		private static IPluginDescription FindUsablePlugin(
			IGrouping<PluginId, KeyValuePair<IPluginDescription, IPluginArchive>> grouping)
		{
			var highestUsable = grouping.Where(x => IsUsable(x.Value.Index)).MaxBy(x => x.Key.Version);
			return highestUsable.Key;
		}

		[Pure]
		public static List<PluginError> FindCompatibilityErrors(IPluginPackageIndex index)
		{
			var errors = new List<PluginError>();
			if (index.PluginArchiveVersion < PluginArchive.MinimumSupportedPluginArchiveVersion)
			{
				// TODO: Make we should make this easier by inserting the current tailviewer version here?
				errors.Add(new PluginError("The plugin targets an older version of Tailviewer and must be compiled against the current version in order to be usable"));
			}

			if (index.PluginArchiveVersion > PluginArchive.CurrentPluginArchiveVersion)
			{
				// TODO: Make we should make this easier by inserting the current tailviewer version here?
				errors.Add(new PluginError("The plugin targets a newer version of Tailviewer and must be compiled against the current version in order to be usable"));
			}

			if (index.ImplementedPluginInterfaces != null)
			{
				foreach (var implementation in index.ImplementedPluginInterfaces)
				{
					if (TryResolvePluginInterface(implementation, out var @interface))
					{
						var currentInterfaceVersion = PluginInterfaceVersionAttribute.GetInterfaceVersion(@interface);
						var implementedInterfaceVersion = new PluginInterfaceVersion(implementation.InterfaceVersion);

						if (implementedInterfaceVersion < currentInterfaceVersion)
						{
							// TODO: Make we should make this easier by inserting the current tailviewer version here?
							errors.Add(new PluginError($"The plugin implements an older version of '{@interface.FullName}'. It must target the current version in order to be usable!"));
						}
						else if (implementedInterfaceVersion > currentInterfaceVersion)
						{
							// TODO: Make we should make this easier by inserting the current tailviewer version here?
							errors.Add(new PluginError($"The plugin implements a newer version of '{@interface.FullName}'. It must target the current version in order to be usable!"));
						}
					}
					else
					{
						// If a plugin unfortunately implements an interface that is not known to this tailviewer,
						// then it is bad news because that means we won't be able to load its assembly!
						errors.Add(new PluginError($"The plugin implements an unknown interface '{implementation.InterfaceTypename}' which is probably part of a newer tailviewer version. The plugin should target the current version in order to be usable!"));
					}
				}
			}

			return errors;
		}

		/// <summary>
		/// Tests if a plugin with the given index is actually usable (or if the plugin is too old, or too new).
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		public static bool IsUsable(IPluginPackageIndex index)
		{
			return !FindCompatibilityErrors(index).Any();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var archive in _archivesByPlugin.Values)
				archive.Dispose();
			_archivesByPlugin.Clear();
		}

		public IPluginStatus GetStatus(IPluginDescription description)
		{
			var id = description?.Id;
			if (id != null && _pluginStati.TryGetValue(id, out var status))
				return status;

			status = new PluginStatus
			{
				IsInstalled = false
			};
			return status;
		}

		public IReadOnlyDictionary<string, Type> ResolveSerializableTypes()
		{
			var serializableTypes = new Dictionary<string, Type>();

			foreach (var pair in _archivesByPlugin)
			{
				var assembly = pair.Value.LoadPlugin();
				var types = pair.Key.SerializableTypes;
				foreach (var tmp in types)
				{
					if (TryResolveType(assembly, tmp.Value, out var type))
					{
						serializableTypes.Add(tmp.Key, type);
					}
				}
			}

			return serializableTypes;
		}

		public T Load<T>(IPluginDescription description) where T : class, IPlugin
		{
			if (!_archivesByPlugin.TryGetValue(description, out var archive))
				throw new ArgumentException();

			if (!description.Plugins.TryGetValue(typeof(T), out var interfaceImplementation))
				throw new NotImplementedException();

			var plugin = archive.LoadPlugin();
			var type = plugin.GetType(interfaceImplementation.FullTypeName);
			var pluginObject = Activator.CreateInstance(type);
			return (T) pluginObject;
		}

		/// <inheritdoc />
		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			var interfaceType = typeof(T);
			Log.InfoFormat("Loading plugins implementing '{0}'...", interfaceType.Name);

			var ret = new List<T>();
			foreach (var pluginDescription in Plugins)
			{
				if (pluginDescription.Plugins.ContainsKey(interfaceType))
				{
					try
					{
						var plugin = Load<T>(pluginDescription);
						ret.Add(plugin);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Unable to load plugin of interface '{0}' from '{1}': {2}",
						                interfaceType,
						                pluginDescription,
						                e);
					}
				}
			}

			Log.InfoFormat("Loaded #{0} plugins", ret.Count);

			return ret;
		}

		public IPluginDescription ReflectPlugin(string pluginPath)
		{
			try
			{
				var archive = PluginArchive.OpenRead(pluginPath);
				return ReflectPlugin(archive);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to load '{0}': {1}", pluginPath, e);

				ExtractIdAndVersion(pluginPath, out var id, out var version);
				var description = new PluginDescription
				{
					Id = id,
					Version = version,
					Error = string.Format("The plugin couldn't be loaded: {0}", e.Message),
					Plugins = new Dictionary<Type, IPluginImplementationDescription>(),
					FilePath = pluginPath
				};
				_archivesByPlugin.Add(description, new EmptyPluginArchive());
				return description;
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

		public static void ExtractIdAndVersion(string pluginPath, out PluginId id, out Version version)
		{
			var fileName = Path.GetFileNameWithoutExtension(pluginPath);
			var tokens = fileName.Split('.').ToList();
			var versionNumbers = new List<int>();
			for (int i = tokens.Count - 1; i >= 0; --i)
			{
				if (int.TryParse(tokens[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var versionNumber))
				{
					versionNumbers.Insert(0, versionNumber);
					tokens.RemoveAt(i);
				}
			}

			if (tokens.Count == 0)
			{
				id = new PluginId("Unknown");
			}
			else
			{
				id = new PluginId(string.Join(".", tokens));
			}

			switch (versionNumbers.Count)
			{
				case 4:
					version = new Version(versionNumbers[0], versionNumbers[1], versionNumbers[2], versionNumbers[3]);
					break;

				case 3:
					version = new Version(versionNumbers[0], versionNumbers[1], versionNumbers[2]);
					break;

				case 2:
					version = new Version(versionNumbers[0], versionNumbers[1]);
					break;

				case 1:
					version = new Version(versionNumbers[0], 0);
					break;

				default:
					version = new Version(0, 0, 0, 0);
					break;
			}
		}

		public IPluginDescription ReflectPlugin(Stream stream, bool leaveOpen = false)
		{
			var archive = PluginArchive.OpenRead(stream, leaveOpen);
			return ReflectPlugin(archive);
		}

		private IPluginDescription ReflectPlugin(PluginArchive archive)
		{
			var description = CreateDescription(archive);
			_archivesByPlugin.Add(description, archive);

			if (!_pluginStati.ContainsKey(description.Id))
			{
				_pluginStati.Add(description.Id, new PluginStatus
				{
					IsInstalled = true
				});
			}

			return description;
		}

		[Pure]
		private static IPluginDescription CreateDescription(PluginArchive archive)
		{
			var archiveIndex = archive.Index;

			Uri.TryCreate(archiveIndex.Website, UriKind.Absolute, out var website);

			var plugins = new Dictionary<Type, IPluginImplementationDescription>();
			foreach (var description in archiveIndex.ImplementedPluginInterfaces)
			{
				var pluginInterfaceType = ResolvePluginInterface(description);
				if (pluginInterfaceType != null)
					plugins.Add(pluginInterfaceType, new PluginImplementationDescription(description));
				else
					Log.WarnFormat("Plugin implements unknown interface '{0}', skipping it...",
					               description.InterfaceTypename);
			}

			var serializableTypes = new Dictionary<string, string>();
			foreach (var pair in archiveIndex.SerializableTypes)
			{
				serializableTypes.Add(pair.Name, pair.FullName);
			}

			var desc = new PluginDescription
			{
				Id = new PluginId(archiveIndex.Id),
				Name = archiveIndex.Name,
				Version = archiveIndex.Version,
				Icon = LoadIcon(archive.ReadIcon()),
				Author = archiveIndex.Author,
				Description = archiveIndex.Description,
				Website = website,
				Plugins = plugins,
				SerializableTypes = serializableTypes
			};

			return desc;
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