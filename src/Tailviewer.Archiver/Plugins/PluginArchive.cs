using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using log4net;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     Responsible for loading a plugin from a tailviewer plugin package.
	/// </summary>
	public sealed class PluginArchive
		: IPluginArchive
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ZipArchive _archive;
		private readonly PluginPackageIndex _index;
		private readonly Dictionary<ZipArchiveEntry, Assembly> _assemblyCache;

		public const string PluginAssemblyEntryName = "Plugin.dll";
		public const string IndexEntryName = "Index.xml";
		public const string IconEntryName = "Icon";
		public const string ChangesName = "Changes.xml";

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
		/// <remarks>
		///     Version 2: Changed TextLogFile ctor, older plugins won't load anymore
		/// </remarks>
		/// <remarks>
		///     Version 3: ILogFile interface has been mostly rewritten to support custom columns and properties
		/// </remarks>
		/// <remarks>
		///     Version 4: PluginPackageIndex stores list of serializable types the plugin implements
		/// </remarks>
		/// <remarks>
		///     Version 5: PluginPackageIndex stores list of changes made in this plugin version.
		///                This change is backwards compatible with v4.
		/// </remarks>
		public const int CurrentPluginArchiveVersion = 5;

		/// <summary>
		///     The currently minimum plugin-archive-version supported by this software.
		/// </summary>
		public const int MinimumSupportedPluginArchiveVersion = 4;

		private PluginArchive(ZipArchive archive)
		{
			if (archive == null)
				throw new ArgumentNullException(nameof(archive));

			var sw = new Stopwatch();

			_archive = archive;
			_index = new PluginPackageIndex();
			_assemblyCache = new Dictionary<ZipArchiveEntry, Assembly>();

			var index = _archive.GetEntry(IndexEntryName);
			if (index == null)
				throw new CorruptPluginException(string.Format("Plugin is missing {0}", IndexEntryName));

			using (var stream = index.Open())
			{
				sw.Restart();
				_index = PluginPackageIndex.Deserialize(stream);
			}
			sw.Stop();

			Log.DebugFormat("Deserialize index took: {0}ms", sw.ElapsedMilliseconds);
		}

		public IPluginPackageIndex Index => _index;

		/// <summary>
		///     Returns a stream to read the icon of this archive from.
		/// </summary>
		/// <returns>A stream pointing towards the icon of this plugin or null the plugin doesn't have an icon</returns>
		public Stream ReadIcon()
		{
			return _archive.GetEntry(IconEntryName)?.Open();
		}

		/// <summary>
		///     Returns a stream to read the plugin's main assembly of this archive from.
		/// </summary>
		/// <returns>A stream pointing towards the main assembly of the plugin</returns>
		public Stream ReadAssembly()
		{
			return _archive.GetEntry(PluginAssemblyEntryName)?.Open();
		}

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

		public IReadOnlyList<SerializableChange> LoadChanges()
		{
			var entry = _archive.GetEntry(PluginArchive.ChangesName);
			if (entry == null)
				return new SerializableChange[0];

			using (var stream = entry.Open())
			{
				try
				{
					return SerializableChanges.Deserialize(stream).Changes;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Unable to load changes: {0}", e);
					return new SerializableChange[0];
				}
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_archive?.Dispose();
		}

		/// <summary>
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static PluginArchive OpenRead(string fileName)
		{
			return new PluginArchive(ZipFile.OpenRead(fileName));
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