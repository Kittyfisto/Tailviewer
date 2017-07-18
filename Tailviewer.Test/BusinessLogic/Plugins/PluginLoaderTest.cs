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
	public sealed class PluginLoaderTest
		: AbstractPluginTest
	{
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

			using (var loader = new PluginLoader())
			{
				var plugin = loader.Load<IFileFormatPlugin>(description);
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

			using (var loader = new PluginLoader())
			{
				var plugin = loader.Load<IFileFormatPlugin>(description);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo2.MyAwesomePlugin");
			}
		}

		[Test]
		[Description("Verifies that both PluginScanner and PluginLoader play nice together")]
		public void TestScanAndLoad()
		{
			using (var scanner = new PluginScanner())
			using (var loader = new PluginLoader())
			{
				var assemblyFileName = "Foo3.tvp";
				if (File.Exists(assemblyFileName))
					File.Delete(assemblyFileName);

				var builder = new PluginBuilder("Foo3", "Simon", "None of your business", "Get of my lawn");
				builder.ImplementInterface<IFileFormatPlugin>("Foo3.MyAwesomePlugin");
				builder.Save();

				var description = scanner.ReflectPlugin(assemblyFileName);
				var plugin = loader.Load<IFileFormatPlugin>(description);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo3.MyAwesomePlugin");
			}
		}

		[Test]
		[Description("Verifies that LoadAllOfType simply skips plugins that cannot be loaded")]
		public void TestLoadAllOfType1()
		{
			using (var loader = new PluginLoader())
			{
				var description = new PluginDescription
				{
					FilePath = "some nonexistant assembly",
					Plugins = new Dictionary<Type, string>
					{
						{typeof(IFileFormatPlugin), "Foo1.MyAwesomePlugin"}
					}
				};

				new Action(() => loader.LoadAllOfType<IFileFormatPlugin>(new[] {description})).ShouldNotThrow();
				loader.LoadAllOfType<IFileFormatPlugin>(new[] {description}).Should().BeEmpty();
			}
		}
	}
}