using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
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
					using (var assembly = new MemoryStream())
					{
						var builder = new PluginBuilder("Plugin", "Simon", "none of your business", "get of my lawn");
						builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
						builder.Save(assembly);
						assembly.Position = 0;

						packer.AddPluginAssembly(assembly);
					}
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
	}
}