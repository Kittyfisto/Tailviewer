using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.AutoUpdates;

namespace Tailviewer.Test.BusinessLogic.AutoUpdater
{
	[TestFixture]
	public sealed class AutoUpdaterTest
	{
		[Test]
		public void TestParse()
		{
			byte[] data = File.ReadAllBytes(@"TestData\query_version.xml");
			VersionInfo version;
			Tailviewer.BusinessLogic.AutoUpdates.AutoUpdater.Parse(data, out version);
			version.Should().NotBeNull();

			version.Beta.Major.Should().Be(0);
			version.Beta.Minor.Should().Be(1);
			version.Beta.Build.Should().Be(80);

			version.Release.Major.Should().Be(0);
			version.Release.Minor.Should().Be(1);
			version.Release.Build.Should().Be(57);
		}
	}
}