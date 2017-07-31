using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public class PluginPackerTest
	{
		private string _fname;

		[SetUp]
		public void Setup()
		{
			_fname = Path.GetTempFileName();
			if (File.Exists(_fname))
				File.Delete(_fname);
		}

		[Test]
		[Ignore("Not yet working")]
		public void TestAddAssembly1()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				packer.AddAssembly("foo", AssemblyFname);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);
				index.Assemblies[0].AssemblyName.Should().Be("Tailviewer.Test");
			}
		}

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static string AssemblyFname
		{
			get
			{
				var name = Path.Combine(AssemblyDirectory, "Tailviewer.Test.dll");
				return name;
			}
		}
	}
}