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
		private string _testData;

		[SetUp]
		public void Setup()
		{
			_fname = Path.GetTempFileName();
			Console.WriteLine("Plugin: {0}", _fname);
			if (File.Exists(_fname))
				File.Delete(_fname);

			_testData = Path.Combine(AssemblyDirectory, "..", "Tailviewer.Archiver.Test", "TestData");
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
				index.NativeImages.Should().NotBeNull();
				index.NativeImages.Should().HaveCount(0);

				var actualAssembly = reader.LoadAssembly("Plugin");
				actualAssembly.Should().NotBeNull();
				actualAssembly.FullName.Should().Be(assembly.FullName);
			}
		}

		[Test]
		public void TestAddNativeImage1()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var fname = Path.Combine(AssemblyDirectory, "archive.exe");
				packer.AddPluginAssembly(fname);

				fname = Path.Combine(_testData, "SharpRemote.PostmortemDebugger.dll");
				packer.AddFile("SharpRemote.PostmortemDebugger.dll", fname);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);

				index.NativeImages.Should().NotBeNull();
				index.NativeImages.Count.Should().Be(1);
				index.NativeImages[0].EntryName.Should().Be("SharpRemote.PostmortemDebugger.dll");
				index.NativeImages[0].ImageName.Should().Be("SharpRemote.PostmortemDebugger");
			}
		}

		[Test]
		[Description("Verifies that adding a 64-bit native image is not supported (yet)")]
		public void TestAddNativeImage2()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var fname = Path.Combine(_testData, "x64", "SharpRemote.PostmortemDebugger.dll");
				new Action(() => packer.AddFile("SharpRemote.PostmortemDebugger.dll", fname))
					.ShouldThrow<NotSupportedException>()
					.WithMessage("ERROR: Native images must be compiled for x86");
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