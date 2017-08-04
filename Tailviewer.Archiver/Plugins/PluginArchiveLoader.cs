using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     This class is responsible for loading plugin archives which have been created with <see cref="PluginPacker" />
	///     (or the cli equivalent, packer.exe).
	/// </summary>
	public sealed class PluginArchiveLoader
		: AbstractPluginLoader
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
		public void Dispose()
		{
			foreach (var archive in _archivesByPlugin.Values)
				archive.Dispose();
			_archivesByPlugin.Clear();
		}

		/// <inheritdoc />
		public override T Load<T>(IPluginDescription description)
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
		public IPluginDescription ReflectPlugin(string pluginPath)
		{
			var archive = PluginArchive.OpenRead(pluginPath);
			var description = CreateDescription(archive.Index);
			_archivesByPlugin.Add(description, archive);
			Add(description);
			return description;
		}

		/// <inheritdoc />
		public IPluginDescription ReflectPlugin(Stream stream, bool leaveOpen = false)
		{
			var archive = PluginArchive.OpenRead(stream, leaveOpen);
			var description = CreateDescription(archive.Index);
			_archivesByPlugin.Add(description, archive);
			Add(description);
			return description;
		}

		[Pure]
		private static IPluginDescription CreateDescription(IPluginPackageIndex archiveIndex)
		{
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
				Author = archiveIndex.Author,
				Description = archiveIndex.Description,
				Website = website,
				Plugins = plugins
			};

			return desc;
		}
	}
}