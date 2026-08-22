using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.AutoUpdates;

namespace Tailviewer.Tests.BusinessLogic.AutoUpdater
{
	[TestFixture]
	public sealed class AutoUpdaterTest
	{
		[Test]
		public void TestParse()
		{
			byte[] data = File.ReadAllBytes(@"TestData\version.json");
			VersionInfo version;
			Tailviewer.BusinessLogic.AutoUpdates.AutoUpdater.Parse(data, out version);
			version.Should().NotBeNull();

			version.Beta.Should().BeNull("because this fork only publishes stable releases");
			version.Stable.Should().Be(new Version(0, 9, 2));
			version.StableAddress.Should().Be(new Uri("https://github.com/brofea/Tailviewer-NEXT/releases/download/v0.9.2/Tailviewer-setup-0.9.2.exe"), "because the setup asset must be preferred over other assets");
		}

		[Test]
		public void TestBuildVersionCheckUri()
		{
			Tailviewer.BusinessLogic.AutoUpdates.AutoUpdater.BuildVersionCheckUri().Should().Be(new Uri("https://api.github.com/repos/brofea/Tailviewer-NEXT/releases/latest"));
		}
	}
}
