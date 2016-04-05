using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class DataSourceTest
	{
		[Test]
		public void TestCtor()
		{
			var dataSource = new DataSource();
			dataSource.ColorByLevel.Should().BeTrue();

			dataSource.ActivatedQuickFilters.Should().NotBeNull();
			dataSource.LevelFilter.Should().Be(LevelFlags.All);
			dataSource.File.Should().BeNull();

			dataSource.VisibleLogLine.Should().Be(LogLineIndex.Invalid);
			dataSource.SelectedLogLine.Should().Be(LogLineIndex.Invalid);

			dataSource.FollowTail.Should().BeFalse();
		}
	}
}