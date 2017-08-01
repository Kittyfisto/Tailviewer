using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public class PluginPackerTest
	{
		private string _fname;

		[SetUp]
		public void Setup()
		{
			_fname = Path.GetTempFileName();
			if (File.Exists(_fname))
				File.Delete(_fname);
		}

		[Test]
		public void TestAddAssembly1()
		{
			var assembly = typeof(IPluginLoader).Assembly;

			using (var packer = PluginPacker.Create(_fname))
			{
				var fname = Path.Combine(AssemblyDirectory, "archive.exe");
				packer.AddPluginAssembly(fname);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);
				index.Assemblies[0].EntryName.Should().Be("Plugin");
				index.Assemblies[0].AssemblyName.Should().Be("archive");
				index.Assemblies[0].AssemblyVersion.Should().Be(assembly.GetName().Version);
				index.Assemblies[0].AssemblyFileVersion.Should().Be(Version.Parse(assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version));
				index.Assemblies[0].AssemblyInformationalVersion.Should().BeNull();

				var actualAssembly = reader.LoadAssembly("Plugin");
				actualAssembly.Should().NotBeNull();
				actualAssembly.FullName.Should().Be(assembly.FullName);
			}
		}

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
				return path;
			}
		}
	}
}