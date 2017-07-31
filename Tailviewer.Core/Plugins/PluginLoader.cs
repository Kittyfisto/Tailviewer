using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class PluginLoader
		: IPluginLoader
	{
		private readonly Dictionary<IPluginDescription, PluginArchive> _plugins;

		/// <summary>
		/// 
		/// </summary>
		public PluginLoader()
		{
			_plugins = new Dictionary<IPluginDescription, PluginArchive>();
		}

		/// <inheritdoc />
		public IReadOnlyList<IPluginDescription> ReflectPlugins(string path)
		{
			var files = Directory.EnumerateFiles(path, "*.tvpp");
			var plugins = new List<IPluginDescription>();
			foreach(var pluginPath in files)
			{
				var plugin = ReflectPlugin(pluginPath);
				plugins.Add(plugin);
			}
			return plugins;
		}

		/// <inheritdoc />
		public IPluginDescription ReflectPlugin(string pluginPath)
		{
			var archive = PluginArchive.OpenRead(pluginPath);
			var description = CreateDescription(archive.Index);
			_plugins.Add(description, archive);
			return description;
		}

		/// <inheritdoc />
		public IPluginDescription ReflectPlugin(Stream stream)
		{
			var archive = PluginArchive.OpenRead(stream);
			var description = CreateDescription(archive.Index);
			_plugins.Add(description, archive);
			return description;
		}

		/// <inheritdoc />
		public T Load<T>(IPluginDescription description) where T : class, IPlugin
		{
			PluginArchive archive;
			if (!_plugins.TryGetValue(description, out archive))
				throw new ArgumentException();

			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public IEnumerable<T> LoadAllOfType<T>(IEnumerable<IPluginDescription> pluginDescriptions) where T : class, IPlugin
		{
			throw new NotImplementedException();
		}

		[Pure]
		private static IPluginDescription CreateDescription(IPluginPackageIndex archiveIndex)
		{
			Uri website;
			Uri.TryCreate(archiveIndex.PluginWebsite, UriKind.Absolute, out website);
			var desc = new PluginDescription
			{
				Author = archiveIndex.PluginAuthor,
				Description = archiveIndex.PluginDescription,
				Website = website
			};
			return desc;
		}
	}
}