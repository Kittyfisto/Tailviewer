using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Tests.Plugins
{
	[TestFixture]
	public sealed class PluginPackageIndexTest
	{
		[Test]
		public void TestVersion()
		{
			IPluginPackageIndex archive = new PluginPackageIndex
			{
				Version = "1.2.3.4"
			};
			archive.Version.Should().Be(new Version(1, 2, 3, 4));
		}

		[Test]
		public void TestTailviewerApiVersion()
		{
			IPluginPackageIndex archive = new PluginPackageIndex
			{
				TailviewerApiVersion = "4.3.2.1"
			};
			archive.TailviewerApiVersion.Should().Be(new Version(4,3,2,1));
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var archive = new PluginPackageIndex
			{
				Assemblies = { },
				ImplementedPluginInterfaces = { },
				NativeImages = { },
				SerializableTypes = { }
			};
			var actualArchive = archive.Roundtrip();
			actualArchive.Assemblies.Should().BeEmpty();
			actualArchive.ImplementedPluginInterfaces.Should().BeEmpty();
			actualArchive.NativeImages.Should().BeEmpty();
			actualArchive.SerializableTypes.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripSimple()
		{
			var archive = new PluginPackageIndex
			{
				Version = "1.2.3.4",
				TailviewerApiVersion = "0.9.8.7",
				Assemblies = new List<AssemblyDescription>
				{
					new AssemblyDescription
					{
						AssemblyFileVersion = "A",
						AssemblyInformationalVersion = "B",
						AssemblyName = "SomeAssembly",
						AssemblyVersion = "C",
						Dependencies = new List<AssemblyReference>
						{
							new AssemblyReference
							{
								FullName = "AnotherAssembly"
							}
						},
						EntryName = "Foobar",
					}
				},
				ImplementedPluginInterfaces = new List<PluginInterfaceImplementation>
				{
					new PluginInterfaceImplementation
					{
						ImplementationTypename = "SomeInterfaceImplementation",
						InterfaceTypename = "SomeInterface"
					}
				},
				NativeImages = new List<NativeImageDescription>
				{
					new NativeImageDescription
					{
						EntryName = "SomeNativeImage",
						ImageName = "Foo.dll"
					}
				},
				SerializableTypes = new List<SerializableTypeDescription>
				{
					new SerializableTypeDescription
					{
						Name = "SomeConfiguration",
						FullName = "A.B.SomeConfiguration"
					}
				}
			};
			var actualArchive = archive.Roundtrip();
			actualArchive.Version.Should().Be("1.2.3.4");
			actualArchive.TailviewerApiVersion.Should().Be("0.9.8.7");
			actualArchive.Assemblies.Should().HaveCount(1);
			actualArchive.Assemblies[0].AssemblyFileVersion.Should().Be("A");
			actualArchive.Assemblies[0].AssemblyInformationalVersion.Should().Be("B");
			actualArchive.Assemblies[0].AssemblyName.Should().Be("SomeAssembly");
			actualArchive.Assemblies[0].AssemblyVersion.Should().Be("C");
			actualArchive.Assemblies[0].Dependencies.Should().HaveCount(1);
			actualArchive.Assemblies[0].Dependencies[0].FullName.Should().Be("AnotherAssembly");
			actualArchive.Assemblies[0].EntryName.Should().Be("Foobar");

			actualArchive.ImplementedPluginInterfaces.Should().HaveCount(1);
			actualArchive.ImplementedPluginInterfaces[0].ImplementationTypename.Should()
				.Be("SomeInterfaceImplementation");
			actualArchive.ImplementedPluginInterfaces[0].InterfaceTypename.Should().Be("SomeInterface");

			actualArchive.NativeImages.Should().HaveCount(1);
			actualArchive.NativeImages[0].EntryName.Should().Be("SomeNativeImage");
			actualArchive.NativeImages[0].ImageName.Should().Be("Foo.dll");

			actualArchive.SerializableTypes.Should().HaveCount(1);
			actualArchive.SerializableTypes[0].Name.Should().Be("SomeConfiguration");
			actualArchive.SerializableTypes[0].FullName.Should().Be("A.B.SomeConfiguration");
		}
	}
}
