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
			byte[] data = File.ReadAllBytes(@"TestData\version.xml");
			VersionInfo version;
			Tailviewer.BusinessLogic.AutoUpdates.AutoUpdater.Parse(data, out version);
			version.Should().NotBeNull();

			version.Beta.Major.Should().Be(0);
			version.Beta.Minor.Should().Be(2);
			version.Beta.Build.Should().Be(67);

			version.Stable.Major.Should().Be(0);
			version.Stable.Minor.Should().Be(2);
			version.Stable.Build.Should().Be(60);
		}
	}
}