using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using DataSource = Tailviewer.Settings.DataSource;

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
			dataSource.ExcludeOther.Should().BeFalse();
			dataSource.File.Should().BeNull();

			dataSource.VisibleEntryIndex.Should().Be(LogEntryIndex.Invalid);
			dataSource.SelectedEntryIndex.Should().Be(LogEntryIndex.Invalid);

			dataSource.FollowTail.Should().BeFalse();
			dataSource.IsOpen.Should().BeFalse();
		}
	}
}