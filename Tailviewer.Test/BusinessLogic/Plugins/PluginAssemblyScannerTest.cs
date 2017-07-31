using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class PluginAssemblyScannerTest
		: AbstractPluginTest
	{
		[Test]
		public void TestReflectPlugin1()
		{
			var plugin = "test.tvp";
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
			var plugin = "sql.tvp";
			if (File.Exists(plugin))
				File.Delete(plugin);

			var builder = new PluginBuilder("sql", "SMI", "none of your business", "go away");
			builder.ImplementInterface<IFileFormatPlugin>("sql.LogFilePlugin");
			builder.Save();

			var scanner = new PluginAssemblyLoader();
			var description = scanner.ReflectPlugin(plugin);
			description.Author.Should().Be("SMI");
			description.Website.Should().Be(new Uri("none of your business", UriKind.RelativeOrAbsolute));
			description.Description.Should().Be("go away");
			description.Plugins.Should().HaveCount(1);
			description.Plugins.Should().Contain(
				new KeyValuePair<Type, string>(typeof(IFileFormatPlugin), "sql.LogFilePlugin, sql, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
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
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugins(@"C:\adwwdwawad\asxas")).ShouldNotThrow();
			scanner.ReflectPlugins(@"C:\adwwdwawad\asxas").Should().BeEmpty();
		}

		[Test]
		public void TestLoad1()
		{
			var assemblyFileName = "Foo1.tvp";
			if (File.Exists(assemblyFileName))
				File.Delete(assemblyFileName);

			var builder = new PluginBuilder("Foo1", "Simon", "None of your business", "Get of my lawn");
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
			var assemblyFileName = "Foo2.tvp";
			if (File.Exists(assemblyFileName))
				File.Delete(assemblyFileName);

			var builder = new PluginBuilder("Foo2", "Simon", "None of your business", "Get of my lawn");
			builder.ImplementInterface<IFileFormatPlugin>("Foo2.MyAwesomePlugin");
			builder.Save();
			var description = new PluginDescription
			{
				FilePath = assemblyFileName,
				Plugins = new Dictionary<Type, string>
				{
					{typeof(IFileFormatPlugin), "Foo2.MyAwesomePlugin, Foo2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"}
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
				var assemblyFileName = "Foo3.tvp";
				if (File.Exists(assemblyFileName))
					File.Delete(assemblyFileName);

				var builder = new PluginBuilder("Foo3", "Simon", "None of your business", "Get of my lawn");
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

				new Action(() => scanner.LoadAllOfType<IFileFormatPlugin>(new[] { description })).ShouldNotThrow();
				scanner.LoadAllOfType<IFileFormatPlugin>(new[] { description }).Should().BeEmpty();
			}
		}
	}
}