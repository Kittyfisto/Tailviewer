using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	///     Responsible for creating a plugin package which can then be dropped into Tailviewer's plugin folder.
	/// </summary>
	/// <remarks>
	///     Plugin authors do not need to this class directly, packer.exe should be more convenient to use in many cases.
	/// </remarks>
	public sealed class PluginPacker
		: IDisposable
	{
		private readonly ZipArchive _archive;
		private readonly PluginPackageIndex _index;
		private bool _disposed;

		private PluginPacker(ZipArchive archive)
		{
			if (archive == null)
				throw new ArgumentNullException(nameof(archive));

			_archive = archive;
			_index = new PluginPackageIndex
			{
				Assemblies = new List<AssemblyDescription>()
			};
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (!_disposed)
			{
				StoreIndex();
				_archive.Dispose();
				_disposed = true;
			}
		}

		private void StoreIndex()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream, Encoding.UTF8, 4086, true))
				{
					var serializer = new XmlSerializer(typeof(PluginPackageIndex));
					serializer.Serialize(writer, _index);
				}

				stream.Position = 0;
				Add("index.xml", stream);
			}
		}

		/// <summary>
		///     Adds a .NET assembly to the plugin package.
		/// </summary>
		/// <param name="entryName"></param>
		/// <param name="fname"></param>
		public void AddAssembly(string entryName, string fname)
		{
			var assemblyDescription = AssemblyDescription.FromFile(fname);
			assemblyDescription.EntryName = entryName;
			using (var stream = File.OpenRead(fname))
			{
				Add(entryName, stream);
			}
			_index.Assemblies.Add(assemblyDescription);
		}

		/// <summary>
		///     Adds a .NET assembly to the plugin package.
		/// </summary>
		/// <param name="entryName"></param>
		/// <param name="assembly"></param>
		public void AddAssembly(string entryName, Stream assembly)
		{
			var rawAssembly = new byte[assembly.Length - assembly.Position];
			int offset = 0;
			int toRead;
			while((toRead = Math.Min(4096, rawAssembly.Length - offset)) > 0)
			{
				offset += assembly.Read(rawAssembly, offset, toRead);
			}
			
			var assemblyDescription = AssemblyDescription.FromRawData(rawAssembly);
			Add(entryName, rawAssembly);
			_index.Assemblies.Add(assemblyDescription);
		}

		/// <summary>
		///     Adds a new file to the  plugin package.
		/// </summary>
		/// <param name="entryName"></param>
		/// <param name="content"></param>
		public void Add(string entryName, Stream content)
		{
			var entry = _archive.CreateEntry(entryName, CompressionLevel.NoCompression);
			using (var stream = entry.Open())
			using (var writer = new BinaryWriter(stream))
			{
				var buffer = new byte[4096];
				int read;
				while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
					writer.Write(buffer, 0, read);
			}
		}

		/// <summary>
		///     Adds a new file to the  plugin package.
		/// </summary>
		/// <param name="entryName"></param>
		/// <param name="content"></param>
		public void Add(string entryName, byte[] content)
		{
			var entry = _archive.CreateEntry(entryName, CompressionLevel.NoCompression);
			using (var stream = entry.Open())
			{
				stream.Write(content, 0, content.Length);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fname"></param>
		/// <returns></returns>
		public static PluginPacker Create(string fname)
		{
			return new PluginPacker(ZipFile.Open(fname, ZipArchiveMode.Create, Encoding.UTF8));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="leaveOpen"></param>
		/// <returns></returns>
		public static PluginPacker Create(Stream stream, bool leaveOpen = false)
		{
			return new PluginPacker(new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen, Encoding.UTF8));
		}
	}
}