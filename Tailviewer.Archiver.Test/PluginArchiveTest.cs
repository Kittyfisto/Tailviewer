using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
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
					packer.AddAssembly("foo", AssemblyFileName);
				}

				stream.Position = 0;

				using (var reader = PluginArchive.OpenRead(stream))
				{
					var assembly = reader.LoadAssembly("foo");
					assembly.Should().NotBeNull();
					reader.LoadAssembly("foo").Should().BeSameAs(assembly, "because LoadAssembly should return the very same assembly every time");
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