using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class PluginArchiveTest
	{
		[Test]
		[Description("Ensures that changing the minimum supported plugin index version is always a conscious decision")]
		public void TestMinimumSupported()
		{
			// If this test fails here then please take a minute and think about whether or not you want to BREAK
			// compatibility with ALL plugins of the previous versions. If no incompatibility was introduced,
			// then it makes no sense to increase this version...
			PluginArchive.MinimumSupportedPluginArchiveVersion.Should().Be(4);
		}

		[Test]
		public void TestLoadAssembly1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("Simon", "Foo1", "TestLoadAssembly1", "Simon", "None of your business", "Get of my lawn");
					builder.ImplementInterface<IFileFormatPlugin>("Foo1.MyAwesomePlugin");
					builder.Save();
					packer.AddPluginAssembly(builder.FileName);
				}

				stream.Position = 0;

				using (var reader = PluginArchive.OpenRead(stream))
				{
					var assembly = reader.LoadAssembly("Plugin.dll");
					assembly.Should().NotBeNull();
					reader.LoadAssembly("Plugin.dll").Should().BeSameAs(assembly, "because LoadAssembly should return the very same assembly every time");
				}
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
	}
}