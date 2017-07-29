using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	///     Responsible for loading a plugin from a tailviewer plugin package.
	/// </summary>
	public sealed class PluginPackage
		: IDisposable
	{
		private readonly ZipArchive _archive;
		private readonly PluginPackageIndex _index;

		private PluginPackage(string fname)
		{
			var sw = new Stopwatch();
			sw.Start();
			_archive = ZipFile.OpenRead(fname);
			sw.Stop();
			Console.WriteLine("OpenRead(): {0}ms", sw.ElapsedMilliseconds);

			_index = new PluginPackageIndex();

			var index = _archive.GetEntry("index.xml");
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

		/// <inheritdoc />
		public void Dispose()
		{
			_archive?.Dispose();
		}

		/// <summary>
		/// </summary>
		/// <param name="fname"></param>
		/// <returns></returns>
		public static PluginPackage OpenRead(string fname)
		{
			return new PluginPackage(fname);
		}
	}
}