using System;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Core.Tests.Sources.Text
{
	[TestFixture]
	public sealed class FileFingerprintTest
	{
		[Test]
		public void TestEqualDifferentFileName()
		{
			var fingerprint1 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);
			var fingerprint2 = new FileFingerprint("C:\\fooo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);

			fingerprint1.Should().NotBe(fingerprint2);
		}

		[Test]
		public void TestEqualDifferentCreationDate()
		{
			var fingerprint1 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);
			var fingerprint2 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 52, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);

			fingerprint1.Should().NotBe(fingerprint2);
		}

		[Test]
		public void TestEqualDifferentLastModifiedDate()
		{
			var fingerprint1 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);
			var fingerprint2 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 04, 49, 21, 31), 41424123131);

			fingerprint1.Should().NotBe(fingerprint2);
		}

		[Test]
		public void TestEqualDifferentFileSize()
		{
			var fingerprint1 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);
			var fingerprint2 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123130);

			fingerprint1.Should().NotBe(fingerprint2);
		}

		[Test]
		public void TestEqual()
		{
			var fingerprint1 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);
			var fingerprint2 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 49, 21, 01), new DateTime(2021, 02, 17, 03, 49, 21, 31), 41424123131);

			fingerprint1.Should().Be(fingerprint2);
		}

		[Test]
		public void TestEqual_DifferentFilenameCasings()
		{
			var fingerprint1 = new FileFingerprint("C:\\foo", new DateTime(2021, 02, 17, 03, 50, 59, 12), new DateTime(2021, 02, 17, 03, 51, 21, 09), 871842142);
			var fingerprint2 = new FileFingerprint("c:\\FoO", new DateTime(2021, 02, 17, 03, 50, 59, 12), new DateTime(2021, 02, 17, 03, 51, 21, 09), 871842142);

			fingerprint1.Should().Be(fingerprint2);
		}
	}
}