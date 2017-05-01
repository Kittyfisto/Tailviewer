using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
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
			dataSource.HideEmptyLines.Should().BeFalse();
			dataSource.IsSingleLine.Should().BeFalse();

			dataSource.ActivatedQuickFilters.Should().NotBeNull();
			dataSource.LevelFilter.Should().Be(LevelFlags.All);
			dataSource.File.Should().BeNull();

			dataSource.VisibleLogLine.Should().Be(LogLineIndex.Invalid);
			dataSource.SelectedLogLines.Should().NotBeNull();
			dataSource.SelectedLogLines.Should().BeEmpty();

			dataSource.FollowTail.Should().BeFalse();

			dataSource.ShowLineNumbers.Should().BeTrue();
		}

		[Test]
		public void TestClone()
		{
			var id = Guid.NewGuid();
			var parent = Guid.NewGuid();
			var filter = Guid.NewGuid();
			var dataSource = new DataSource
			{
				VisibleLogLine = new LogLineIndex(42),
				LastViewed = new DateTime(2017, 5, 1, 17, 4, 0),
				Order = 10,
				HideEmptyLines = true,
				File = @"F:\foo.db",
				Id = id,
				ShowLineNumbers = true,
				HorizontalOffset = 101,
				ColorByLevel = true,
				FollowTail = true,
				IsSingleLine = true,
				LevelFilter = LevelFlags.Fatal,
				ParentId = parent,
				SearchTerm = "stuff",
				SelectedLogLines =
				{
					new LogLineIndex(1),
					new LogLineIndex(10)
				},
				ActivatedQuickFilters = {filter}
			};
			var cloned = dataSource.Clone();
			cloned.Should().NotBeNull();
			cloned.Should().NotBeSameAs(dataSource);
			cloned.VisibleLogLine.Should().Be(new LogLineIndex(42));
			cloned.LastViewed.Should().Be(new DateTime(2017, 5, 1, 17, 4, 0));
			cloned.Order.Should().Be(10);
			cloned.HideEmptyLines.Should().BeTrue();
			cloned.File.Should().Be(@"F:\foo.db");
			cloned.Id.Should().Be(id);
			cloned.ShowLineNumbers.Should().BeTrue();
			cloned.HorizontalOffset.Should().Be(101);
			cloned.ColorByLevel.Should().BeTrue();
			cloned.FollowTail.Should().BeTrue();
			cloned.IsSingleLine.Should().BeTrue();
			cloned.LevelFilter.Should().Be(LevelFlags.Fatal);
			cloned.ParentId.Should().Be(parent);
			cloned.SearchTerm.Should().Be("stuff");
			cloned.SelectedLogLines.Should().BeEquivalentTo(new LogLineIndex(1), new LogLineIndex(10));
			cloned.SelectedLogLines.Should().NotBeSameAs(dataSource.SelectedLogLines);
			cloned.ActivatedQuickFilters.Should().Equal(new object[] {filter});
			cloned.ActivatedQuickFilters.Should().NotBeSameAs(dataSource.ActivatedQuickFilters);
		}
	}
}