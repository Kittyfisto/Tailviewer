using System;
using System.IO;
using System.Reflection;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     <see cref="IPluginArchive" /> implementation for an empty archive, used when a plugin
	///     failed to be loaded.
	/// </summary>
	public sealed class EmptyPluginArchive
		: IPluginArchive
	{
		private readonly PluginPackageIndex _index;

		public EmptyPluginArchive(Version pluginVersion)
		{
			_index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion,
				Version = pluginVersion.ToString()
			};
		}

		public IPluginPackageIndex Index => _index;

		public Stream ReadIcon()
		{
			return null;
		}

		public Assembly LoadAssembly(string entryName)
		{
			return null;
		}

		public Assembly LoadPlugin()
		{
			return null;
		}

		public void Dispose()
		{
		}
	}
}