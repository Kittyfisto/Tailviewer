using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class PluginArchiveTest
	{
		[Test]
		public void TestLoadAssembly1()
		{
			using (var stream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(stream, true))
				{
					var builder = new PluginBuilder("Simon", "Foo1", "Foo1", "Simon", "None of your business", "Get of my lawn");
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