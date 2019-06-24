using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.SystemTests.Plugins.v0._8._0._685
{
	[TestFixture]
	public sealed class PluginCompatabilityTest
		: AbstractPluginCompatabilityTest
	{
		[SetUp]
		public void Setup()
		{
			// Tests which execute Tailviewer.exe interefere with each other:
			// We introduce some timeout in between those tests so they are more likely
			// to succeed (until a proper fix has been implemented).
			Thread.Sleep(TimeSpan.FromMilliseconds(500));
		}

		[Test]
		public void TestLoadFileFormatPlugin2()
		{
			// CMD> Tailviewer.PluginCreator.exe FileFormatPlugin  Tailviewer.BusinessLogic.Plugins.IFileFormatPlugin 0.8.0.680-beta
			ShouldNotBeLoaded<IFileFormatPlugin2>(new Version(0, 8, 0, 685), "FileFormatPlugin2.0.0.tvp");
		}
	}
}
