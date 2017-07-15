using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;

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

			var loader = new PluginLoader();
			var plugin = loader.Load<IFileFormatPlugin>(description);
			plugin.Should().NotBeNull();
			plugin.GetType().FullName.Should().Be("Foo1.MyAwesomePlugin");
		}
	}
}