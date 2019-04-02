using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test.Plugins
{
	[TestFixture]
	public sealed class PluginGroupTest
	{
		[Test]
		public void TestPluginIndexTooOld()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.MinimumSupportedPluginArchiveVersion - 1
			};
			PluginGroup.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError("The plugin targets an older version of Tailviewer and must be compiled against the current version in order to be usable")
			});
		}

		[Test]
		public void TestPluginIndexTooNew()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion + 1
			};
			PluginGroup.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError("The plugin targets a newer version of Tailviewer and must be compiled against the current version in order to be usable")
			});
		}

		[Test]
		[Description("Verifies that an error is generated if a plugin has been implemented against an older interface version")]
		public void TestPluginInterfaceTooOld()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion,
				ImplementedPluginInterfaces = new List<PluginInterfaceImplementation>
				{
					new PluginInterfaceImplementation
					{
						InterfaceVersion = 1, // It's an older code, sir, and doesn't check out
						InterfaceTypename = typeof(IDataSourceAnalyserPlugin).FullName
					}
				}
			};
			PluginGroup.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError($"The plugin implements an older version of '{typeof(IDataSourceAnalyserPlugin).FullName}'. It must target the current version in order to be usable!")
			});
		}

		[Test]
		[Description("Verifies that an error is generated if a plugin has been implemented against a newer interface version")]
		public void TestPluginInterfaceTooNew()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion,
				ImplementedPluginInterfaces = new List<PluginInterfaceImplementation>
				{
					new PluginInterfaceImplementation
					{
						InterfaceVersion = 11, // It's a newer code, sir, and doesn't check out
						InterfaceTypename = typeof(IDataSourceAnalyserPlugin).FullName
					}
				}
			};
			PluginGroup.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError($"The plugin implements a newer version of '{typeof(IDataSourceAnalyserPlugin).FullName}'. It must target the current version in order to be usable!")
			});
		}

		interface IFancyPantsInterface
			: IPlugin
		{}

		[Test]
		public void TestPluginInterfaceUnknown()
		{
			var index = new PluginPackageIndex
			{
				PluginArchiveVersion = PluginArchive.CurrentPluginArchiveVersion,
				ImplementedPluginInterfaces = new List<PluginInterfaceImplementation>
				{
					new PluginInterfaceImplementation
					{
						InterfaceVersion = 1,
						InterfaceTypename = typeof(IFancyPantsInterface).FullName //< That interface won't be loaded by the PluginArchiverLoader because it's not part of the API project
					}
				}
			};
			PluginGroup.FindCompatibilityErrors(index).Should().BeEquivalentTo(new object[]
			{
				new PluginError($"The plugin implements an unknown interface '{typeof(IFancyPantsInterface).FullName}' which is probably part of a newer tailviewer version. The plugin should target the current version in order to be usable!")
			});
		}
	}
}
