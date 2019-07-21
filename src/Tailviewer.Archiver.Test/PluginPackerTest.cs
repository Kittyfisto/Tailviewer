using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Test;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public class PluginPackerTest
	{
		private string _fname;
		private string _testData;

		[SetUp]
		public void Setup()
		{
			_fname = Path.GetTempFileName();
			Console.WriteLine("Plugin: {0}", _fname);
			if (File.Exists(_fname))
				File.Delete(_fname);

			_testData = Path.Combine(AssemblyDirectory, "..", "src", "Tailviewer.Archiver.Test", "TestData");
		}

		[Test]
		public void TestAddAssembly1()
		{
			using (var packer = CreatePacker(_fname))
			{
				var builder = new PluginBuilder("Simon", "Foo", "Plugin");
				builder.AssemblyVersion = "4.0.3.1";
				builder.AssemblyFileVersion = "1.2.3.42";
				builder.AssemblyInformationalVersion = "4.0.0.0-beta";
				builder.Save();
				packer.AddPluginAssembly(builder.FileName);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.Name.Should().Be("Plugin");
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);
				index.Assemblies[0].EntryName.Should().Be("Plugin.dll");
				index.Assemblies[0].AssemblyName.Should().Be("Plugin");
				index.Assemblies[0].AssemblyVersion.Should().Be(new Version(4, 0, 3, 1));
				index.Assemblies[0].AssemblyFileVersion.Should().Be(new Version(1, 2, 3, 42));
				index.Assemblies[0].AssemblyInformationalVersion.Should().Be("4.0.0.0-beta");
				index.NativeImages.Should().NotBeNull();
				index.NativeImages.Should().HaveCount(0);

				var actualAssembly = reader.LoadAssembly("Plugin.dll");
				actualAssembly.Should().NotBeNull();
			}
		}

		[Test]
		[Description("Verifies that adding x64 assemblies is not supported (yet)")]
		public void TestAddAssembly2()
		{
			using (var packer = CreatePacker(_fname))
			{
				var fname = Path.Combine(_testData, "Managed", "x64", "ClassLibrary1.dll");
				new Action(() => packer.AddFile("ClassLibrary1.dll", fname))
					.Should().Throw<PackException>()
					.WithMessage("Assemblies must be compiled for x86 or AnyCPU");
			}
		}

		[Test]
		[Description("Verifies that adding x86 assemblies is allowed")]
		public void TestAddAssembly3()
		{
			using (var packer = CreatePacker(_fname))
			{
				var fname = Path.Combine(_testData, "Managed", "x86", "ClassLibrary1.dll");
				packer.AddFile("ClassLibrary1.dll", fname);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.PluginArchiveVersion.Should().Be(PluginArchive.CurrentPluginArchiveVersion);
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);
				index.Assemblies[0].EntryName.Should().Be("ClassLibrary1.dll");
				index.Assemblies[0].AssemblyName.Should().Be("ClassLibrary1");
			}
		}

		[Test]
		[Description("Verifies that plugins may not target newer frameworks than 4.5.2")]
		public void TestAddAssembly4()
		{
			using (var packer = CreatePacker(_fname))
			{
				var fname = Path.Combine(_testData, "Managed", "x86", "Targets.NET.4.6.dll");
				new Action(() => packer.AddFile("Foo.dll", fname))
					.Should().Throw<PackException>()
					.WithMessage("Assemblies may only target frameworks of up to .NET 4.5.2");

				fname = Path.Combine(_testData, "Managed", "x86", "Targets.NET.4.6.1.dll");
				new Action(() => packer.AddFile("Foo.dll", fname))
					.Should().Throw<PackException>()
					.WithMessage("Assemblies may only target frameworks of up to .NET 4.5.2");

				fname = Path.Combine(_testData, "Managed", "x86", "Targets.NET.4.6.2.dll");
				new Action(() => packer.AddFile("Foo.dll", fname))
					.Should().Throw<PackException>()
					.WithMessage("Assemblies may only target frameworks of up to .NET 4.5.2");
			}
		}

		[Test]
		public void TestAddAssembly5()
		{
			using (var packer = CreatePacker(_fname))
			{
				var builder = new PluginBuilder("Kittyfisto", "MyPlugin", "My First Plugin");
				builder.PluginVersion = new Version(1, 4, 12034);
				builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
				builder.Save();

				packer.AddPluginAssembly(builder.FileName);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Version.Should().NotBeNull();
				index.Id.Should().Be("Kittyfisto.MyPlugin");
				index.Name.Should().Be("My First Plugin");
				index.Version.Should().Be(new Version(1, 4, 12034));
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/179")]
		public void TestAddSameAssemblyTwice()
		{
			var directory = Directory.GetCurrentDirectory();
			var aFileName = "A.dll";
			var aFilePath = Path.Combine(directory, aFileName);
			var bFileName = "B.dll";
			var bFilePath = Path.Combine(directory, bFileName);
			using (var packer = CreatePacker(_fname))
			{
				var aBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("A"), AssemblyBuilderAccess.Save);
				var aModule = aBuilder.DefineDynamicModule(aFileName);
				var aType = aModule.DefineType("A_Dummy", TypeAttributes.Class | TypeAttributes.Public);
				aType.CreateType();
				aBuilder.Save(aFileName);

				var bBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("B"), AssemblyBuilderAccess.Save);
				var bModule = bBuilder.DefineDynamicModule(bFileName);
				var bType = bModule.DefineType("B_Dummy", TypeAttributes.Class | TypeAttributes.Public, Assembly.LoadFile(aFilePath).ExportedTypes.First());
				bType.CreateType();
				bBuilder.Save(bFileName);

				var builder = new PluginBuilder("Simon", "Foo", "Plugin");
				builder.AddDependency(aFilePath);
				builder.AddDependency(bFilePath);
				builder.Save();
				packer.AddPluginAssembly(builder.FileName);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var assemblies = reader.Index.Assemblies;
				assemblies.Should().ContainSingle(x => x.AssemblyName == "A");
				assemblies.Should().ContainSingle(x => x.AssemblyName == "B");
				assemblies.Should().ContainSingle(x => x.AssemblyName == "Plugin");
				assemblies.Should().HaveCount(3, "because we expect a total of 3 assemblies to have been saved");
			}
		}

		[Test]
		public void TestAddNativeImage1()
		{
			using (var packer = CreatePacker(_fname))
			{
				var builder = new PluginBuilder("Simon", "Foo", "Plugin");
				builder.Save();
				packer.AddPluginAssembly(builder.FileName);

				var fname = Path.Combine(_testData, "Native", "x86", "NativeImage.dll");
				packer.AddFile("NativeImage.dll", fname);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);

				index.NativeImages.Should().NotBeNull();
				index.NativeImages.Count.Should().Be(1);
				index.NativeImages[0].EntryName.Should().Be("NativeImage.dll");
				index.NativeImages[0].ImageName.Should().Be("NativeImage");
			}
		}

		[Test]
		[Description("Verifies that adding a 64-bit native image is not supported (yet)")]
		public void TestAddNativeImage2()
		{
			using (var packer = CreatePacker(_fname))
			{
				var fname = Path.Combine(_testData, "Native", "x64", "NativeImage.dll");
				new Action(() => packer.AddFile("NativeImage.dll", fname))
					.Should().Throw<PackException>()
					.WithMessage("Native images must be compiled for x86");
			}
		}

		[Test]
		public void TestAddIcon1()
		{
			using (var packer = CreatePacker(_fname))
			{
				using (var icon = File.OpenRead(Path.Combine(_testData, "cropped-uiforetwicon2.png")))
				{
					packer.SetIcon(icon);
				}
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var stream = reader.ReadIcon();
				stream.Should().NotBeNull();

				using (var image = new Bitmap(stream))
				{
					image.Width.Should().Be(16);
					image.Height.Should().Be(16);
				}
			}
		}

		[Test]
		public void TestAddChanges()
		{
			using (var packer = CreatePacker(_fname))
			{
				var fileName = Path.GetTempFileName();
				using (var writer = XmlWriter.Create(fileName))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("changelist");
					writer.WriteStartElement("changes");

					writer.WriteStartElement("change");
					writer.WriteAttributeString("summary", "foo");
					writer.WriteEndElement();

					writer.WriteStartElement("change");
					writer.WriteAttributeString("summary", "bar");
					writer.WriteAttributeString("description", "A very detailed\ndescription");
					writer.WriteEndElement();

					writer.WriteEndElement();
					writer.WriteEndElement();
				}

				packer.SetChanges(fileName);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var changes = reader.LoadChanges();
				changes.Should().HaveCount(2);
				changes[0].Summary.Should().Be("foo");
				changes[0].Description.Should().BeNullOrEmpty();

				changes[1].Summary.Should().Be("bar");
				changes[1].Description.Should().Be("A very detailed\ndescription");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fname"></param>
		/// <returns></returns>
		private static PluginPacker CreatePacker(string fname)
		{
			return PluginPacker.Create(File.OpenWrite(fname));
		}

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
				return path;
			}
		}
	}
}