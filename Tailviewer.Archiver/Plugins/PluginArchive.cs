using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     Responsible for loading a plugin from a tailviewer plugin package.
	/// </summary>
	public sealed class PluginArchive
		: IDisposable
	{
		private readonly ZipArchive _archive;
		private readonly PluginPackageIndex _index;
		private readonly Dictionary<ZipArchiveEntry, Assembly> _assemblyCache;
		private Assembly _pluginAssembly;

		public const string PluginAssemblyEntryName = "Plugin.dll";
		public const string IndexEntryName = "Index.xml";

		/// <summary>
		///     The file-extension of the plugin, excluding the dot.
		/// </summary>
		public const string PluginExtension = "tvp";

		/// <summary>
		///     The current version of a plugin archive, if created with this software.
		/// </summary>
		/// <remarks>
		///     This number will be increased while plugins are developed and the
		///     <see cref="MinimumSupportedPluginArchiveVersion" /> will probably increase
		///     with it until version 1.0 of Tailviewer is released.
		/// </remarks>
		/// <remarks>
		///     Plugins with a newer version will not be supported.
		/// </remarks>
		public const int CurrentPluginArchiveVersion = 1;

		/// <summary>
		///     The currently minimum plugin-archive-version supported by this software.
		/// </summary>
		public const int MinimumSupportedPluginArchiveVersion = CurrentPluginArchiveVersion;
		
		private PluginArchive(ZipArchive archive)
		{
			if (archive == null)
				throw new ArgumentNullException(nameof(archive));

			var sw = new Stopwatch();

			_archive = archive;
			_index = new PluginPackageIndex();
			_assemblyCache = new Dictionary<ZipArchiveEntry, Assembly>();

			var index = _archive.GetEntry(IndexEntryName);
			using (var stream = index.Open())
			using (var reader = new StreamReader(stream))
			{
				var serializer = new XmlSerializer(typeof(PluginPackageIndex));
				sw.Restart();
				_index = serializer.Deserialize(reader) as PluginPackageIndex;
			}
			sw.Stop();
			Console.WriteLine("Deserialize index: {0}ms", sw.ElapsedMilliseconds);
		}

		public IPluginPackageIndex Index => _index;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entryName"></param>
		/// <returns></returns>
		public Assembly LoadAssembly(string entryName)
		{
			var entry = _archive.GetEntry(entryName);
			if (entry == null)
				throw new FileNotFoundException(string.Format("Unable to find '{0}' in the plugin", entryName), entryName);

			Assembly assembly;
			if (!_assemblyCache.TryGetValue(entry, out assembly))
			{
				var rawAssembly = new byte[entry.Length];
				using (var stream = entry.Open())
				{
					int read = 0;
					int offset = 0;
					int size;
					do
					{
						offset += read;
						size = Math.Min(4096, rawAssembly.Length - offset);
					} while ((read = stream.Read(rawAssembly, offset, size)) > 0);
				}
				assembly = Assembly.Load(rawAssembly);
				_assemblyCache.Add(entry, assembly);
			}

			return assembly;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Assembly LoadPlugin()
		{
			return LoadAssembly(PluginAssemblyEntryName);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_archive?.Dispose();
		}

		/// <summary>
		/// </summary>
		/// <param name="fname"></param>
		/// <returns></returns>
		public static PluginArchive OpenRead(string fname)
		{
			return new PluginArchive(ZipFile.OpenRead(fname));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="leaveOpen"></param>
		/// <returns></returns>
		public static PluginArchive OpenRead(Stream stream, bool leaveOpen = false)
		{
			return new PluginArchive(new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen, Encoding.UTF8));
		}
	}
}