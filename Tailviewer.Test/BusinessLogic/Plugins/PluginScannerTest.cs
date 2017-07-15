using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class PluginScannerTest
		: AbstractPluginTest
	{
		[Test]
		public void TestReflectPlugin1()
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

		[Test]
		public void TestReflectPlugin2()
		{
			var scanner = new PluginScanner();
			new Action(() => scanner.ReflectPlugin(null)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestReflectPlugin3()
		{
			var scanner = new PluginScanner();
			new Action(() => scanner.ReflectPlugin("DAAWDADAWWF")).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestReflectPlugin4()
		{
			var scanner = new PluginScanner();
			new Action(() => scanner.ReflectPlugin(@"C:\adwwdwawad\asxas")).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestReflectPlugin5()
		{
			var scanner = new PluginScanner();
			new Action(() => scanner.ReflectPlugin("C:\adwwdwawad\asxas")).ShouldThrow<ArgumentException>(
				"because we used illegal characters in that path");
		}
	}
}