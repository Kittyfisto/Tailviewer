using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Applications;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class GitGetChangesTest
	{
		[Test]
		public void TestGetCommitAfterLastTag()
		{
			GitGetChanges.GetCommitAfterLastTag("v0.8.1-41-gbb5529c5")
			             .Should().Be("HEAD~39");
		}

		[Test]
		public void TestFilterIncludeOnly()
		{
			var changes = new SerializableChanges
			{
				Changes =
				{
					new SerializableChange {Summary = "Rimworld"},
					new SerializableChange {Summary = "VA-11"},
					new SerializableChange {Summary = "Whatever", Description = "Hey, VA-11 is a pretty good game"}
				}
			};
			var filtered = GitGetChanges.Filter(changes, "VA", null);
			filtered.Should().NotBeNull();
			filtered.Changes.Should().HaveCount(2);
			filtered.Changes[0].Summary.Should().Be("VA-11");
			filtered.Changes[1].Summary.Should().Be("Whatever");
			filtered.Changes[1].Description.Should().Be("Hey, VA-11 is a pretty good game");
		}

		[Test]
		public void TestFilterExcludeOnly()
		{
			var changes = new SerializableChanges
			{
				Changes =
				{
					new SerializableChange {Summary = "Rimworld"},
					new SerializableChange {Summary = "VA-11"}
				}
			};
			var filtered = GitGetChanges.Filter(changes, null, "VA");
			filtered.Should().NotBeNull();
			filtered.Changes.Should().HaveCount(1);
			filtered.Changes[0].Summary.Should().Be("Rimworld");
		}

		[Test]
		public void TestFilterIncludeExclude()
		{
			var changes = new SerializableChanges
			{
				Changes =
				{
					new SerializableChange {Summary = "Bugfix for merge"},
					new SerializableChange {Summary = "Bugfix test"}
				}
			};
			var filtered = GitGetChanges.Filter(changes, "bugfix", "merge");
			filtered.Should().NotBeNull();
			filtered.Changes.Should().HaveCount(1);
			filtered.Changes[0].Summary.Should().Be("Bugfix test");
		}
	}
}
