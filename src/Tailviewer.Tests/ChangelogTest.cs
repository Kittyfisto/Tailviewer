using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Tests
{
	[TestFixture]
	public sealed class ChangelogTest
	{
		[Test]
		[Description("Verifies that the changelog doesn't reference the same version or date more than once")]
		public void TestUniqueness()
		{
			var changes = Changelog.Changes;
			var dates = new HashSet<DateTime>(changes.Select(x => x.ReleaseDate));
			dates.Count.Should().Be(changes.Count, "because no two releases should've happened on the same date");
			var versions = new HashSet<Version>(changes.Select(x => x.Version));
			versions.Count.Should().Be(changes.Count, "because no two releases should've had the same version number");
		}
	}
}