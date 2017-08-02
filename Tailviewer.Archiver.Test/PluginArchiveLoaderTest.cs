using System.IO;
using System.Linq;
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