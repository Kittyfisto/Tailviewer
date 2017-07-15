using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class PluginScannerTest
	{
		[Test]
		public void TestPlugin()
		{
			var plugin = "test.tvp";
			if (File.Exists(plugin))
				File.Delete(plugin);

			CreatePlugin(plugin, "John Snow", "https://got.com", "You know nothing, John Snow!");
			var scanner = new PluginScanner();
			var description = scanner.ReflectPlugin(plugin);
			description.Should().NotBeNull();
			description.FilePath.Should().Be(plugin);
			description.Author.Should().Be("John Snow");
			description.Website.Should().Be(new Uri("https://got.com", UriKind.RelativeOrAbsolute));
			description.Description.Should().Be("You know nothing, John Snow!");
			description.Plugins.Should().NotBeNull();
			description.Plugins.Should().BeEmpty("Because we didn't add any plugin implementations");
		}

		private void CreatePlugin(string assemblyFileName, string author, string website, string description)
		{
			var pluginName = Path.GetFileNameWithoutExtension(assemblyFileName);
			var assemblyName = new AssemblyName(pluginName);
			var attributes = new List<CustomAttributeBuilder>();
			if (author != null)
				attributes.Add(CreateAttribute<PluginAuthorAttribute>(author));
			if (website != null)
				attributes.Add(CreateAttribute<PluginWebsiteAttribute>(website));
			if (website != null)
				attributes.Add(CreateAttribute<PluginDescriptionAttribute>(description));
			var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, attributes);
			var module = assembly.DefineDynamicModule(pluginName);
			assembly.Save(assemblyFileName);
		}

		private static CustomAttributeBuilder CreateAttribute<T>(params object[] parameters) where T : Attribute
		{
			var type = typeof(T);
			var ctors = type.GetConstructors();
			var builder = new CustomAttributeBuilder(ctors.First(), parameters);
			return builder;
		}
	}
}