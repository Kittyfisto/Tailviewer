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
		private InMemoryFilesystem _filesystem;

		[SetUp]
		public void Setup()
		{
			_pluginFolder = Path.Combine(Path.GetDirectoryName(AssemblyFileName), "Plugins", "PluginArchiveLoaderTest");
			if (!Directory.Exists(_pluginFolder))
				Directory.CreateDirectory(_pluginFolder);
			else
				DeleteContents(_pluginFolder);

			_filesystem = new InMemoryFilesystem();
			_filesystem.AddRoot(Path.GetPathRoot(Constants.PluginPath));
			_filesystem.CreateDirectory(Constants.PluginPath);
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
		public void TestGetPluginStatusForNonPlugin()
		{
			using (var loader = new PluginArchiveLoader(_filesystem, Constants.PluginPath))
			{
				var status = loader.GetStatus(null);
				status.Should().NotBeNull();
				status.IsInstalled.Should().BeFalse();
				status.IsLoaded.Should().BeFalse();


				status = loader.GetStatus(new PluginId("dawawd"));
				status.Should().NotBeNull();
				status.IsInstalled.Should().BeFalse();
				status.IsLoaded.Should().BeFalse();
			}
		}

		[Test]
		public void TestGetPluginStatusForInstalledPlugin()
		{
			using (var stream = new MemoryStream())
			{
				var id = CreatePlugin(stream);
				_filesystem.Write(Path.Combine(Constants.PluginPath, $"{id}.2.tvp"), stream);

				using (var loader = new PluginArchiveLoader(_filesystem, Constants.PluginPath))
				{
					loader.Plugins.Should().HaveCount(1, "because one plugin should've been loaded");
					var status = loader.GetStatus(id);
					status.Should().NotBeNull();
					status.IsInstalled.Should().BeTrue("because we've just installed that plugin");
					status.IsLoaded.Should().BeTrue("because we successfully loaded the plugin");
					status.LoadException.Should().BeNull("because we successfully loaded the plugin");
				}
			}
		}

		[Test]
		public void TestReflect1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("Kittyfisto", "UniquePluginId", "My very own plugin", "Simon", "http://google.com", "get of my lawn");
					builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
					builder.Save();

					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;
				_filesystem.Write(Path.Combine(Constants.PluginPath, "Kittyfisto.UniquePluginId.2.0.tvp"), stream);

				using (var loader = new PluginArchiveLoader(_filesystem, Constants.PluginPath))
				{
					loader.Plugins.Should().HaveCount(1, "because one plugin should've been loaded");
					var description = loader.Plugins.First();
					description.Should().NotBeNull();
					description.Id.Should().Be(new PluginId("Kittyfisto.UniquePluginId"));
					description.Name.Should().Be("My very own plugin");
					description.Version.Should().Be(new Version(0, 0, 0), "because the plugin version should default to 0.0.0 when none has been specified");
					description.Author.Should().Be("Simon");
					description.Website.Should().Be(new Uri("http://google.com"));
					description.Description.Should().Be("get of my lawn");
				}
			}
		}

		[Test]
		[Description("Verifies that ReflectPlugin() doesn't throw when a plugin couldn't be loaded and instead returns a descriptor with as much information as can be obtained")]
		public void TestReflect2()
		{
			var path = Path.Combine(Constants.PluginPath, "BrokenPlugin.1.0.2.4.tvp");
			_filesystem.CreateFile(path);
			using (var loader = new PluginArchiveLoader(_filesystem, Constants.PluginPath))
			{
				loader.Plugins.Should().HaveCount(1, "because we've created one plugin");
				var description = loader.Plugins.First();
				description.Should().NotBeNull();
				description.Author.Should().BeNull("because the author cannot be known");
				description.Description.Should().BeNull("because we couldn't extract a description from the plugin");
				description.FilePath.Should().Be(path);
				description.Id.Should().Be(new PluginId("BrokenPlugin"), "because the id should've been extracted from the path");
				description.Error.Should().NotBeNull("because the plugin couldn't be loaded");
				description.PluginImplementations.Should().NotBeNull();
				description.PluginImplementations.Should().BeEmpty();
				description.Version.Should().Be(new Version(1, 0, 2, 4), "because the version should've been extracted from the path");
				description.Icon.Should().BeNull();
				description.Website.Should().BeNull();
			}
		}

		[Test]
		public void TestLoad1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("Kittyfisto", "Simon", "none of your business", "get of my lawn");
					builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
					builder.Save();

					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;
				var path = Path.Combine(Constants.PluginPath, "Kittyfisto.Simon.1.0.tvp");
				_filesystem.WriteAllBytes(path, stream.ToArray());

				using (var loader = new PluginArchiveLoader(_filesystem, Constants.PluginPath))
				{
					var plugins = loader.LoadAllOfType<IFileFormatPlugin>();
					plugins.Should().HaveCount(1, "because we've added one plugin which implements the IFileFormatPlugin interface");
				}
			}
		}

		[Test]
		[Description("Verifies that if the same plugin is available in two versions, then the highest possible version will be available only")]
		public void TestLoadDifferentVersions()
		{
			CreatePlugin("Kittyfisto", "Foobar", new Version(1, 0));
			CreatePlugin("Kittyfisto", "Foobar", new Version(1, 1));

			using (var loader = new PluginArchiveLoader(_filesystem, Constants.PluginPath))
			{
				loader.Plugins.Should().HaveCount(1);
				loader.Plugins.First().Version.Should().Be(new Version(1, 1));

				var plugins = loader.LoadAllOfType<IFileFormatPlugin>();
				plugins.Should().HaveCount(1);
			}
		}

		[Test]
		public void TestLoadAllOfType1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("Kittyfisto", "SomePlugin", "none of your business", "get of my lawn");
					builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
					builder.Save();

					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;
				_filesystem.Write(Path.Combine(Constants.PluginPath, "Kittyfisto.SomePlugin.1.tvp"), stream);

				using (var loader = new PluginArchiveLoader(_filesystem, Constants.PluginPath))
				{
					var plugins = loader.LoadAllOfType<IFileFormatPlugin>()?.ToList();
					plugins.Should().NotBeNull();
					plugins.Should().HaveCount(1);
					plugins[0].Should().NotBeNull();
					plugins[0].GetType().FullName.Should().Be("Plugin.FileFormatPlugin");
				}
			}
		}

		[Test]
		public void TestExtractIdAndVersion1()
		{
			PluginArchiveLoader.ExtractIdAndVersion("SamplePlugin.1.2.3.tvp", out var id, out var version);
			id.Should().Be(new PluginId("SamplePlugin"));
			version.Should().Be(new Version(1, 2, 3));
		}

		[Test]
		public void TestExtractIdAndVersion2()
		{
			PluginArchiveLoader.ExtractIdAndVersion("Tailviewer.BrokenPlugin.3.2.tvp", out var id, out var version);
			id.Should().Be(new PluginId("Tailviewer.BrokenPlugin"));
			version.Should().Be(new Version(3, 2));
		}

		private static PluginId CreatePlugin(MemoryStream stream)
		{
			var @namespace = "Kittyfisto";
			var name = "UniquePluginId";
			using (var packer = PluginPacker.Create(stream, true))
			{
				var builder = new PluginBuilder(@namespace, name, "My very own plugin", "Simon", "http://google.com",
				                                "get of my lawn");
				builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
				builder.Save();

				packer.AddPluginAssembly(builder.FileName);
			}

			stream.Position = 0;
			return new PluginId($"{@namespace}.{name}");
		}

		private void CreatePlugin(string @namespace, string name, Version version)
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, leaveOpen: true))
				{
					var builder = new PluginBuilder(@namespace, name, "dawawdwdaaw") { PluginVersion = version };
					builder.ImplementInterface<IFileFormatPlugin>("dwwwddwawa");
					builder.Save();
					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;
				var fileName = Path.Combine(Constants.PluginPath, string.Format("{0}.{1}.{2}.tvp", @namespace, name, version));
				_filesystem.Write(fileName, stream);
			}
		}
	}
}