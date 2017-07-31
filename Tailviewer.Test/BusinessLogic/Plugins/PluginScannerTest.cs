using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class PluginScannerTest
		: AbstractPluginTest
	{
		[Test]
		[Ignore("not yet implemented")]
		public void TestReflectPlugin1()
		{
			using (var plugin = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(plugin, true))
				using (var assembly = new MemoryStream())
				{
					var builder = new PluginBuilder("foo", "micky mouse", "https://google.com", "some plugin");
					builder.ImplementInterface<IFileFormatPlugin>("stuff");
					builder.Save(assembly);

					assembly.Position = 0;
					packer.AddAssembly("too.tvp", assembly);
				}

				plugin.Position = 0;
				var scanner = new PluginScanner();
				var description = scanner.ReflectPlugin(plugin);
				description.Should().NotBeNull();
				description.Author.Should().Be("micky mouse");
			}
		}
	}
}