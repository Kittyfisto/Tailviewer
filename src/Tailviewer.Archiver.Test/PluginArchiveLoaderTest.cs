using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Analysis;
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
			using (var loader = new PluginArchiveLoader(_filesystem, null))
			{
				var status = loader.GetStatus(null);
				status.Should().NotBeNull();
				status.IsInstalled.Should().BeFalse();
				status.IsLoaded.Should().BeFalse();


				var description = new Mock<IPluginDescription>();
				status = loader.GetStatus(description.Object);
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
				CreatePlugin(stream);

				using (var loader = new PluginArchiveLoader(_filesystem, null))
				{
					var description = loader.ReflectPlugin(stream, true);
					var status = loader.GetStatus(description);
					status.Should().NotBeNull();
					status.IsInstalled.Should().BeTrue("because we've just installed that plugin");
					status.IsLoaded.Should().BeFalse("because we haven't tried to load the plugin just yet");
					status.LoadException.Should().BeNull("because we haven't tried to load the plugin just yet");
				}
			}
		}



		private static void CreatePlugin(MemoryStream stream)
		{
			using (var packer = PluginPacker.Create(stream, true))
			{
				var builder = new PluginBuilder("Kittyfisto", "UniquePluginId", "My very own plugin", "Simon", "http://google.com",
					"get of my lawn");
				builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
				builder.Save();

				packer.AddPluginAssembly(builder.FileName);
			}

			stream.Position = 0;
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

				using (var loader = new PluginArchiveLoader(_filesystem, null))
				{
					var description = loader.ReflectPlugin(stream, true);
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
			using (var loader = new PluginArchiveLoader(_filesystem, null))
			{
				var description = loader.ReflectPlugin("C:\\BrokenPlugin.1.0.2.4.tvp");
				description.Should().NotBeNull();
				description.Author.Should().BeNull("because the author cannot be known");
				description.Description.Should().BeNull("because we couldn't extract a description from the plugin");
				description.FilePath.Should().Be("C:\\BrokenPlugin.1.0.2.4.tvp");
				description.Id.Should().Be(new PluginId("BrokenPlugin"), "because the id should've been extracted from the path");
				description.Error.Should().NotBeNull("because the plugin couldn't be loaded");
				description.Plugins.Should().NotBeNull();
				description.Plugins.Should().BeEmpty();
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

				using (var loader = new PluginArchiveLoader(_filesystem, null))
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
			var plugin1 = CreatePlugin("Kittyfisto", "Foobar", new Version(1, 0));
			var plugin2 = CreatePlugin("Kittyfisto", "Foobar", new Version(1, 1));

			using (var loader = new PluginArchiveLoader(_filesystem, null))
			{
				loader.ReflectPlugin(plugin1);
				loader.ReflectPlugin(plugin2);

				loader.Plugins.Should().HaveCount(1);
				loader.Plugins.First().Version.Should().Be(new Version(1, 1));

				var plugins = loader.LoadAllOfType<IFileFormatPlugin>();
				plugins.Should().HaveCount(1);
			}
		}

		private string CreatePlugin(string @namespace, string name, Version version)
		{
			var fileName = Path.Combine(_pluginFolder, string.Format("{0}.{1}.{2}.dll", @namespace, name, version));
			using (var packer = PluginPacker.Create(fileName))
			{
				var builder = new PluginBuilder(@namespace, name, "dawawdwdaaw") { PluginVersion = version };
				builder.ImplementInterface<IFileFormatPlugin>("dwwwddwawa");
				builder.Save();
				packer.AddPluginAssembly(builder.FileName);
			}

			var dest = Path.Combine(_pluginFolder, Path.GetFileName(fileName));
			File.Move(fileName, dest);

			return dest;
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

				using (var loader = new PluginArchiveLoader(_filesystem, null))
				{
					var description = loader.ReflectPlugin(stream, true);
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

		[Test]
		public void TestPluginIndexTooOld()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.MinimumSupportedPluginArchiveVersion - 1
			};
			PluginArchiveLoader.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError("The plugin targets an older version of Tailviewer and must be compiled against the current version in order to be usable")
			});
		}

		[Test]
		public void TestPluginIndexTooNew()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion + 1
			};
			PluginArchiveLoader.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError("The plugin targets a newer version of Tailviewer and must be compiled against the current version in order to be usable")
			});
		}

		[Test]
		[Description("Verifies that an error is generated if a plugin has been implemented against an older interface version")]
		public void TestPluginInterfaceTooOld()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion,
				ImplementedPluginInterfaces = new List<PluginInterfaceImplementation>
				{
					new PluginInterfaceImplementation
					{
						InterfaceVersion = 1, // It's an older code, sir, and doesn't check out
						InterfaceTypename = typeof(IDataSourceAnalyserPlugin).FullName
					}
				}
			};
			PluginArchiveLoader.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError($"The plugin implements an older version of '{typeof(IDataSourceAnalyserPlugin).FullName}'. It must target the current version in order to be usable!")
			});
		}

		[Test]
		[Description("Verifies that an error is generated if a plugin has been implemented against a newer interface version")]
		public void TestPluginInterfaceTooNew()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion,
				ImplementedPluginInterfaces = new List<PluginInterfaceImplementation>
				{
					new PluginInterfaceImplementation
					{
						InterfaceVersion = 11, // It's a newer code, sir, and doesn't check out
						InterfaceTypename = typeof(IDataSourceAnalyserPlugin).FullName
					}
				}
			};
			PluginArchiveLoader.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError($"The plugin implements a newer version of '{typeof(IDataSourceAnalyserPlugin).FullName}'. It must target the current version in order to be usable!")
			});
		}

		interface IFancyPantsInterface
			: IPlugin
		{}

		[Test]
		public void TestPluginInterfaceUnknown()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion,
				ImplementedPluginInterfaces = new List<PluginInterfaceImplementation>
				{
					new PluginInterfaceImplementation
					{
						InterfaceVersion = 1,
						InterfaceTypename = typeof(IFancyPantsInterface).FullName //< That interface won't be loaded by the PluginArchiverLoader because it's not part of the API project
					}
				}
			};
			PluginArchiveLoader.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError($"The plugin implements an unknown interface '{typeof(IFancyPantsInterface).FullName}' which is probably part of a newer tailviewer version. The plugin should target the current version in order to be usable!")
			});
		}
	}
}