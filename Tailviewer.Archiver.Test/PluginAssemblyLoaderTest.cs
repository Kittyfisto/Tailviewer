using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class PluginAssemblyLoaderTest
		: AbstractPluginTest
	{
		[Test]
		public void TestReflectPlugin1()
		{
			var plugin = "test.dll";
			if (File.Exists(plugin))
				File.Delete(plugin);

			CreatePlugin(plugin, "John Snow", "https://got.com", "You know nothing, John Snow!");
			var scanner = new PluginAssemblyLoader();
			var description = scanner.ReflectPlugin(plugin);
			description.Should().NotBeNull();
			description.FilePath.Should().Be(plugin);
			description.Author.Should().Be("John Snow");
			description.Website.Should().Be(new Uri("https://got.com", UriKind.RelativeOrAbsolute));
			description.Description.Should().Be("You know nothing, John Snow!");
			description.Plugins.Should().NotBeNull();
			description.Plugins.Should().BeEmpty("Because we didn't add any plugin implementations");
		}

		[Test]
		public void TestReflectPlugin2()
		{
			var plugin = "sql.dll";
			if (File.Exists(plugin))
				File.Delete(plugin);

			var builder = new PluginBuilder("sql", "sql", "SMI", "none of your business", "go away");
			builder.ImplementInterface<IFileFormatPlugin>("sql.LogFilePlugin");
			builder.Save();

			var scanner = new PluginAssemblyLoader();
			var description = scanner.ReflectPlugin(plugin);
			description.Author.Should().Be("SMI");
			description.Website.Should().Be(new Uri("none of your business", UriKind.RelativeOrAbsolute));
			description.Description.Should().Be("go away");
			description.Plugins.Should().HaveCount(1);
			description.Plugins.Should().Contain(
				new KeyValuePair<Type, string>(typeof(IFileFormatPlugin), "sql.LogFilePlugin")
			);
		}

		[Test]
		public void TestReflectPlugin3()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin(null)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestReflectPlugin4()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin("DAAWDADAWWF")).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestReflectPlugin5()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin(@"C:\adwwdwawad\asxas")).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestReflectPlugin6()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin("C:\adwwdwawad\asxas")).ShouldThrow<ArgumentException>(
				"because we used illegal characters in that path");
		}

		[Test]
		public void TestReflectPlugins()
		{
			var scanner = new PluginAssemblyLoader(@"C:\adwwdwawad\asxas");
			scanner.Plugins.Should().BeEmpty();
		}

		[Test]
		public void TestLoad1()
		{
			var assemblyFileName = "Foo1.dll";
			if (File.Exists(assemblyFileName))
				File.Delete(assemblyFileName);

			var builder = new PluginBuilder("Foo1", "Foo1", "Simon", "None of your business", "Get of my lawn");
			builder.ImplementInterface<IFileFormatPlugin>("Foo1.MyAwesomePlugin");
			builder.Save();
			var description = new PluginDescription
			{
				FilePath = assemblyFileName,
				Plugins = new Dictionary<Type, string>
				{
					{typeof(IFileFormatPlugin), "Foo1.MyAwesomePlugin"}
				}
			};

			using (var scanner = new PluginAssemblyLoader())
			{
				var plugin = scanner.Load<IFileFormatPlugin>(description);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo1.MyAwesomePlugin");
			}
		}

		[Test]
		public void TestLoad2()
		{
			var assemblyFileName = "Foo2.dll";
			if (File.Exists(assemblyFileName))
				File.Delete(assemblyFileName);

			var builder = new PluginBuilder("Foo2", "Foo2", "Simon", "None of your business", "Get of my lawn");
			builder.ImplementInterface<IFileFormatPlugin>("Foo2.MyAwesomePlugin");
			builder.Save();
			var description = new PluginDescription
			{
				FilePath = assemblyFileName,
				Plugins = new Dictionary<Type, string>
				{
					{typeof(IFileFormatPlugin), "Foo2.MyAwesomePlugin"}
				}
			};

			using (var scanner = new PluginAssemblyLoader())
			{
				var plugin = scanner.Load<IFileFormatPlugin>(description);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo2.MyAwesomePlugin");
			}
		}

		[Test]
		[Description("Verifies that both PluginScanner and PluginLoader play nice together")]
		public void TestScanAndLoad()
		{
			using (var scanner = new PluginAssemblyLoader())
			{
				var assemblyFileName = "Foo3.dll";
				if (File.Exists(assemblyFileName))
					File.Delete(assemblyFileName);

				var builder = new PluginBuilder("Foo3", "Foo3", "Simon", "None of your business", "Get of my lawn");
				builder.ImplementInterface<IFileFormatPlugin>("Foo3.MyAwesomePlugin");
				builder.Save();

				var description = scanner.ReflectPlugin(assemblyFileName);
				var plugin = scanner.Load<IFileFormatPlugin>(description);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo3.MyAwesomePlugin");
			}
		}

		[Test]
		[Description("Verifies that LoadAllOfType simply skips plugins that cannot be loaded")]
		public void TestLoadAllOfType1()
		{
			using (var scanner = new PluginAssemblyLoader())
			{
				var description = new PluginDescription
				{
					FilePath = "some nonexistant assembly",
					Plugins = new Dictionary<Type, string>
					{
						{typeof(IFileFormatPlugin), "Foo1.MyAwesomePlugin"}
					}
				};

				new Action(() => scanner.LoadAllOfType<IFileFormatPlugin>()).ShouldNotThrow();
				scanner.LoadAllOfType<IFileFormatPlugin>().Should().BeEmpty();
			}
		}
	}
}