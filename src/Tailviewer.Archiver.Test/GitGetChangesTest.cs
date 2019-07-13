using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Applications;

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
	}
}
