using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
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

		private readonly Dictionary<IPluginDescription, PluginArchive> _archivesByPlugin;

		/// <summary>
		/// </summary>
		public PluginArchiveLoader(string path = null)
		{
			_archivesByPlugin = new Dictionary<IPluginDescription, PluginArchive>();

			try
			{
				if (path != null)
				{
					var files = Directory.EnumerateFiles(path, string.Format("*.{0}", PluginArchive.PluginExtension));
					foreach (var pluginPath in files)
						ReflectPlugin(pluginPath);
				}
			}
			catch (DirectoryNotFoundException e)
			{
				Log.WarnFormat("Unable to find plugins in '{0}': {1}", path, e);
			}
		}

		/// <inheritdoc />
		public IEnumerable<IPluginDescription> Plugins
		{
			get
			{
				var plugins = _archivesByPlugin.GroupBy(x => x.Key.Id).Select(FindUsablePlugin).Where(x => x != null).ToList();
				return plugins;
			}
		}

		[Pure]
		private static IPluginDescription FindUsablePlugin(IGrouping<string, KeyValuePair<IPluginDescription, PluginArchive>> grouping)
		{
			var highestUsable = grouping.Where(x => IsUsable(x.Value.Index)).MaxBy(x => x.Key.Version);
			return highestUsable.Key;
		}

		/// <summary>
		/// Tests if a plugin with the given index is actually usable (or if the plugin is too old, or too new).
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		private static bool IsUsable(IPluginPackageIndex index)
		{
			if (index.PluginArchiveVersion < PluginArchive.MinimumSupportedPluginArchiveVersion)
				return false;

			if (index.PluginArchiveVersion > PluginArchive.CurrentPluginArchiveVersion)
				return false;

			// We might also want to discard plugins which are built against outdated APIs, but this
			// will probably have to be hard-coded, no?
			return true;
		}

		public IEnumerable<IPluginDescription> AllPlugins => _archivesByPlugin.Keys.ToList();

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var archive in _archivesByPlugin.Values)
				archive.Dispose();
			_archivesByPlugin.Clear();
		}

		/// <inheritdoc />
		public T Load<T>(IPluginDescription description) where T : class, IPlugin
		{
			PluginArchive archive;
			if (!_archivesByPlugin.TryGetValue(description, out archive))
				throw new ArgumentException();

			string interfaceImplementation;
			if (!description.Plugins.TryGetValue(typeof(T), out interfaceImplementation))
				throw new NotImplementedException();

			var plugin = archive.LoadPlugin();
			var type = plugin.GetType(interfaceImplementation);
			var pluginObject = Activator.CreateInstance(type);
			return (T) pluginObject;
		}

		/// <inheritdoc />
		public IEnumerable<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			var ret = new List<T>();
			foreach (var pluginDescription in Plugins)
			{
				if (pluginDescription.Plugins.ContainsKey(typeof(T)))
				{
					try
					{
						var plugin = Load<T>(pluginDescription);
						ret.Add(plugin);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Unable to load plugin of interface '{0}' from '{1}': {2}",
							typeof(T),
							pluginDescription,
							e);
					}
				}
			}
			return ret;
		}

		/// <inheritdoc />
		public IPluginDescription ReflectPlugin(string pluginPath)
		{
			var archive = PluginArchive.OpenRead(pluginPath);
			var description = CreateDescription(archive);
			_archivesByPlugin.Add(description, archive);
			return description;
		}

		/// <inheritdoc />
		public IPluginDescription ReflectPlugin(Stream stream, bool leaveOpen = false)
		{
			var archive = PluginArchive.OpenRead(stream, leaveOpen);
			var description = CreateDescription(archive);
			_archivesByPlugin.Add(description, archive);
			return description;
		}

		[Pure]
		private static IPluginDescription CreateDescription(PluginArchive archive)
		{
			var archiveIndex = archive.Index;

			Uri website;
			Uri.TryCreate(archiveIndex.Website, UriKind.Absolute, out website);

			var plugins = new Dictionary<Type, string>();
			foreach (var pair in archiveIndex.ImplementedPluginInterfaces)
			{
				var pluginInterfaceType = typeof(IPlugin).Assembly.GetType(pair.InterfaceTypename);
				if (pluginInterfaceType != null)
					plugins.Add(pluginInterfaceType, pair.ImplementationTypename);
				else
					Log.WarnFormat("Plugin implements unknown interface '{0}', skipping it...", pair.InterfaceTypename);
			}

			var desc = new PluginDescription
			{
				Id = archiveIndex.Id,
				Name = archiveIndex.Name,
				Version = archiveIndex.Version,
				Icon = LoadIcon(archive.ReadIcon()),
				Author = archiveIndex.Author,
				Description = archiveIndex.Description,
				Website = website,
				Plugins = plugins
			};

			return desc;
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