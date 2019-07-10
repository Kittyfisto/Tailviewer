using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

		private readonly Dictionary<PluginId, PluginGroup> _plugins;
		private readonly IFilesystem _filesystem;

		/// <summary>
		/// </summary>
		/// <param name="filesystem"></param>
		/// <param name="pluginPaths"></param>
		public PluginArchiveLoader(IFilesystem filesystem, params string[] pluginPaths)
		{
			_filesystem = filesystem;
			_plugins = new Dictionary<PluginId, PluginGroup>();

			try
			{
				// TODO: How would we make this truly async? Currently the app has to block until all plugins are loaded wich is sad
				foreach (var path in pluginPaths)
				{
					TryLoadPluginsFrom(filesystem, path);
				}

				foreach (var plugin in _plugins.Values)
					plugin.Load();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught exception while trying to load plugins:\r\n{0}", e);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var pluginGroup in _plugins.Values)
				pluginGroup.Dispose();
			_plugins.Clear();
		}

		/// <inheritdoc />
		public IEnumerable<IPluginDescription> Plugins
		{
			get { return _plugins.Values.Select(x => x.Description).Where(x => x != null).ToList(); }
		}

		public IPluginStatus GetStatus(PluginId id)
		{
			if (id != null && _plugins.TryGetValue(id, out var group))
				return group.Status;

			return new PluginStatus
			{
				IsInstalled = false
			};
		}

		public IReadOnlyDictionary<string, Type> ResolveSerializableTypes()
		{
			var serializableTypes = new Dictionary<string, Type>();

			foreach (var plugin in _plugins.Values)
			foreach (var pair in plugin.ResolveSerializableTypes())
				serializableTypes.Add(pair.Key, pair.Value);

			return serializableTypes;
		}

		/// <inheritdoc />
		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			return LoadAllOfTypeWithDescription<T>().Select(x => x.Plugin).ToList();
		}

		public IReadOnlyList<IPluginWithDescription<T>> LoadAllOfTypeWithDescription<T>() where T : class, IPlugin
		{
			var interfaceType = typeof(T);
			Log.InfoFormat("Loading plugins implementing '{0}'...", interfaceType.Name);

			var ret = new List<IPluginWithDescription<T>>();
			foreach (var id in _plugins.Keys) ret.AddRange(LoadAllOfTypeFrom<T>(id));

			Log.InfoFormat("Loaded #{0} plugins", ret.Count);

			return ret;
		}

		[Pure]
		public IEnumerable<IPluginWithDescription<T>> LoadAllOfTypeFrom<T>(PluginId id) where T : class, IPlugin
		{
			if (!_plugins.TryGetValue(id, out var plugin))
				return Enumerable.Empty<PluginWithDescription<T>>();

			return plugin.LoadAllOfType<T>();
		}

		/// <summary>
		///     Tries to open the file at the given path as a tailviewer plugin.
		///     Succeeds if the file is a zip archive and its metadata could be read.
		///     The plugin itself is not loaded until later.
		/// </summary>
		/// <param name="pluginPath"></param>
		public void TryOpenPlugin(string pluginPath)
		{
			Log.DebugFormat("Loading plugin '{0}'...", pluginPath);

			ExtractIdAndVersion(pluginPath, out var id, out var version);
			try
			{
				OpenPlugin(pluginPath, id, version);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to load '{0}': {1}", pluginPath, e);
				Add(pluginPath, id, version, new EmptyPluginArchive(version));
			}
		}

		public PluginGroup OpenPlugin(string pluginPath, PluginId id, Version version)
		{
			var stream = _filesystem.OpenRead(pluginPath);
			try
			{
				// Ownership of the stream transfers to PluginArchive which is disposed of when this class is
				// disposed of....
				var archive = PluginArchive.OpenRead(stream);
				return Add(pluginPath, id, version, archive);
			}
			catch (Exception)
			{
				stream.Dispose();
				throw;
			}
		}

		private void TryLoadPluginsFrom(IFilesystem filesystem, string path)
		{
			try
			{
				var files = filesystem.EnumerateFiles(path, string.Format("*.{0}", PluginArchive.PluginExtension));
				foreach (var pluginPath in files)
					TryOpenPlugin(pluginPath);
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

		private PluginGroup Add(string pluginPath, PluginId id, Version pluginVersion, IPluginArchive archive)
		{
			if (!_plugins.TryGetValue(id, out var pluginGroup))
			{
				pluginGroup = new PluginGroup(id);
				_plugins.Add(id, pluginGroup);
			}

			pluginGroup.Add(pluginPath, pluginVersion, archive);
			return pluginGroup;
		}

		public static void ExtractIdAndVersion(string pluginPath, out PluginId id, out Version version)
		{
			var fileName = Path.GetFileNameWithoutExtension(pluginPath);
			var tokens = fileName.Split('.').ToList();
			var versionNumbers = new List<int>();
			for (var i = tokens.Count - 1; i >= 0; --i)
				if (int.TryParse(tokens[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var versionNumber))
				{
					versionNumbers.Insert(index: 0, item: versionNumber);
					tokens.RemoveAt(i);
				}

			if (tokens.Count == 0)
				id = new PluginId("Unknown");
			else
				id = new PluginId(string.Join(".", tokens));

			switch (versionNumbers.Count)
			{
				case 4:
					version = new Version(versionNumbers[index: 0], versionNumbers[index: 1], versionNumbers[index: 2],
					                      versionNumbers[index: 3]);
					break;

				case 3:
					version = new Version(versionNumbers[index: 0], versionNumbers[index: 1], versionNumbers[index: 2]);
					break;

				case 2:
					version = new Version(versionNumbers[index: 0], versionNumbers[index: 1]);
					break;

				case 1:
					version = new Version(versionNumbers[index: 0], minor: 0);
					break;

				default:
					version = new Version(major: 0, minor: 0, build: 0, revision: 0);
					break;
			}
		}
	}
}