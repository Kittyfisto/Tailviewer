using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class PluginArchiveLoaderTest
		: AbstractPluginTest
	{
		private string _pluginFolder;

		[SetUp]
		public void Setup()
		{
			_pluginFolder = Path.Combine(Path.GetDirectoryName(AssemblyFileName), "Plugins", "PluginArchiveLoaderTest");
			if (!Directory.Exists(_pluginFolder))
				Directory.CreateDirectory(_pluginFolder);
			else
				DeleteContents(_pluginFolder);
		}

		private static void DeleteContents(string pluginFolder)
		{
			var directory = new DirectoryInfo(pluginFolder);

			foreach (FileInfo file in directory.GetFiles())
			{
				file.Delete();
			}
		}

		public static string AssemblyFileName
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return path;
			}
		}

		[Test]
		public void TestReflect1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("UniquePluginId", "My very own plugin", "Simon", "http://google.com", "get of my lawn");
					builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
					builder.Save();

					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;

				using (var loader = new PluginArchiveLoader())
				{
					var description = loader.ReflectPlugin(stream, true);
					description.Should().NotBeNull();
					description.Id.Should().Be("UniquePluginId");
					description.Name.Should().Be("My very own plugin");
					description.Version.Should().Be(new Version(0, 0, 0));
					description.Author.Should().Be("Simon");
					description.Website.Should().Be(new Uri("http://google.com"));
					description.Description.Should().Be("get of my lawn");
				}
			}
		}

		[Test]
		public void TestLoad1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("Plugin", "Simon", "none of your business", "get of my lawn");
					builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
					builder.Save();

					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;

				using (var loader = new PluginArchiveLoader())
				{
					var description = loader.ReflectPlugin(stream, true);
					var plugin = loader.Load<IFileFormatPlugin>(description);
					plugin.Should().NotBeNull();
				}
			}
		}

		[Test]
		[Description("Verifies that if the same plugin is available in two versions, then the highest possible version will be available only")]
		public void TestLoadDifferentVersions()
		{
			
		}

		private string CreatePlugin(string id, Version version)
		{
			var builder = new PluginBuilder(id, "dawawdwdaaw") {PluginVersion = version};
			builder.Save();
			var fname = builder.FileName;
			var dest = Path.Combine(_pluginFolder, Path.GetFileName(fname));
			File.Move(fname, dest);
			return dest;
		}

		[Test]
		public void TestLoadAllOfType1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("Plugin", "Simon", "none of your business", "get of my lawn");
					builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
					builder.Save();

					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;

				using (var loader = new PluginArchiveLoader())
				{
					var description = loader.ReflectPlugin(stream, true);
					var plugins = loader.LoadAllOfType<IFileFormatPlugin>(new[] {description})?.ToList();
					plugins.Should().NotBeNull();
					plugins.Should().HaveCount(1);
					plugins[0].Should().NotBeNull();
					plugins[0].GetType().FullName.Should().Be("Plugin.FileFormatPlugin");
				}
			}
		}
	}
}