using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
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
			dataSource.ActivatedQuickFilters.Should().BeEmpty();
			dataSource.Analyses.Should().NotBeNull();
			dataSource.Analyses.Should().BeEmpty();

			dataSource.LevelFilter.Should().Be(LevelFlags.All);
			dataSource.File.Should().BeNull();

			dataSource.VisibleLogLine.Should().Be(LogLineIndex.Invalid);
			dataSource.SelectedLogLines.Should().NotBeNull();
			dataSource.SelectedLogLines.Should().BeEmpty();

			dataSource.FollowTail.Should().BeFalse();

			dataSource.ShowLineNumbers.Should().BeTrue();

			dataSource.IsExpanded.Should().BeTrue();
		}

		public static IEnumerable<bool> Bool => new[] {true, false};

		[Test]
		public void TestClone([ValueSource(nameof(Bool))] bool hideEmptyLines,
			[ValueSource(nameof(Bool))] bool showLineNumbers,
			[ValueSource(nameof(Bool))] bool colorByLevel,
			[ValueSource(nameof(Bool))] bool followTail,
			[ValueSource(nameof(Bool))] bool isSingleLine,
			[ValueSource(nameof(Bool))] bool isExpanded)
		{
			var id = DataSourceId.CreateNew();
			var parent = DataSourceId.CreateNew();
			var filter = Guid.NewGuid();
			var dataSource = new DataSource
			{
				VisibleLogLine = new LogLineIndex(42),
				LastViewed = new DateTime(2017, 5, 1, 17, 4, 0),
				Order = 10,
				HideEmptyLines = hideEmptyLines,
				File = @"F:\foo.db",
				Id = id,
				ShowLineNumbers = showLineNumbers,
				HorizontalOffset = 101,
				ColorByLevel = colorByLevel,
				FollowTail = followTail,
				IsSingleLine = isSingleLine,
				IsExpanded = isExpanded,
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
			cloned.HideEmptyLines.Should().Be(hideEmptyLines);
			cloned.File.Should().Be(@"F:\foo.db");
			cloned.Id.Should().Be(id);
			cloned.ShowLineNumbers.Should().Be(showLineNumbers);
			cloned.HorizontalOffset.Should().Be(101);
			cloned.ColorByLevel.Should().Be(colorByLevel);
			cloned.FollowTail.Should().Be(followTail);
			cloned.IsSingleLine.Should().Be(isSingleLine);
			cloned.IsExpanded.Should().Be(isExpanded);
			cloned.LevelFilter.Should().Be(LevelFlags.Fatal);
			cloned.ParentId.Should().Be(parent);
			cloned.SearchTerm.Should().Be("stuff");
			cloned.SelectedLogLines.Should().BeEquivalentTo(new LogLineIndex(1), new LogLineIndex(10));
			cloned.SelectedLogLines.Should().NotBeSameAs(dataSource.SelectedLogLines);
			cloned.ActivatedQuickFilters.Should().Equal(new object[] {filter});
			cloned.ActivatedQuickFilters.Should().NotBeSameAs(dataSource.ActivatedQuickFilters);
		}

		[Test]
		public void TestSaveRestore([ValueSource(nameof(Bool))] bool hideEmptyLines,
			[ValueSource(nameof(Bool))] bool showLineNumbers,
			[ValueSource(nameof(Bool))] bool colorByLevel,
			[ValueSource(nameof(Bool))] bool followTail,
			[ValueSource(nameof(Bool))] bool isSingleLine,
			[ValueSource(nameof(Bool))] bool isExpanded)
		{
			var id = DataSourceId.CreateNew();
			var parent = DataSourceId.CreateNew();
			var filter = Guid.NewGuid();

			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");

					var dataSource = new DataSource
					{
						VisibleLogLine = new LogLineIndex(42),
						LastViewed = new DateTime(2017, 5, 1, 17, 4, 0),
						//Order = 10,
						HideEmptyLines = hideEmptyLines,
						File = @"F:\foo.db",
						Id = id,
						ShowLineNumbers = showLineNumbers,
						HorizontalOffset = 101,
						ColorByLevel = colorByLevel,
						FollowTail = followTail,
						IsSingleLine = isSingleLine,
						IsExpanded = isExpanded,
						LevelFilter = LevelFlags.Fatal,
						ParentId = parent,
						SearchTerm = "stuff",
						//SelectedLogLines =
						//{
						//	new LogLineIndex(1),
						//	new LogLineIndex(10)
						//},
						ActivatedQuickFilters = { filter }
					};
					dataSource.Save(writer);
				}
				stream.Position = 0;
				Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var dataSource = new DataSource();
					bool unused;
					dataSource.Restore(reader, out unused);
					dataSource.VisibleLogLine.Should().Be(new LogLineIndex(42));
					dataSource.LastViewed.Should().Be(new DateTime(2017, 5, 1, 17, 4, 0));
					//dataSource.Order.Should().Be(10);
					dataSource.HideEmptyLines.Should().Be(hideEmptyLines);
					dataSource.File.Should().Be(@"F:\foo.db");
					dataSource.Id.Should().Be(id);
					dataSource.ShowLineNumbers.Should().Be(showLineNumbers);
					dataSource.HorizontalOffset.Should().Be(101);
					dataSource.ColorByLevel.Should().Be(colorByLevel);
					dataSource.FollowTail.Should().Be(followTail);
					dataSource.IsSingleLine.Should().Be(isSingleLine);
					dataSource.IsExpanded.Should().Be(isExpanded);
					dataSource.LevelFilter.Should().Be(LevelFlags.Fatal);
					dataSource.ParentId.Should().Be(parent);
					dataSource.SearchTerm.Should().Be("stuff");
					//dataSource.SelectedLogLines.Should().BeEquivalentTo(new LogLineIndex(1), new LogLineIndex(10));
					dataSource.ActivatedQuickFilters.Should().Equal(new object[] { filter });
				}
			}
		}
	}
}