using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Ui.Controls.LogView;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	public sealed class TextLineTest
	{
		private HashSet<LogLineIndex> _hovered;
		private HashSet<LogLineIndex> _selected;

		[SetUp]
		public void SetUp()
		{
			_hovered = new HashSet<LogLineIndex>();
			_selected = new HashSet<LogLineIndex>();
		}

		[Test]
		public void TestColorByLevel1()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, false);
			textLine.ColorByLevel.Should().BeFalse();
		}

		[Test]
		public void TestColorByLevel2()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.ColorByLevel.Should().BeTrue();
		}

		[Test]
		public void TestColorByLevel3()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.ColorByLevel = false;
			textLine.ColorByLevel.Should().BeFalse();
		}

		[Test]
		public void TestHighlight1()
		{
			var results = new SearchResults();
			results.Add(new LogMatch(0, new LogLineMatch(1, 2)));

			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true)
				{
					SearchResults = results
				};
			var segments = textLine.Segments.ToList();
			segments.Count.Should().Be(3);
			segments[0].FormattedText.Text.Should().Be("f");
			segments[1].FormattedText.Text.Should().Be("oo");
			segments[2].FormattedText.Text.Should().Be("bar");
		}

		[Test]
		public void TestHighlight2()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			var segments = textLine.Segments.ToList();
			segments.Count.Should().Be(1);
			segments[0].FormattedText.Text.Should().Be("foobar");
		}

		[Test]
		[Description("Verifies that changing the filter on a TextLine results in segments changing")]
		public void TestHighlight3()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.Segments.Count().Should().Be(1);
			textLine.Segments.First().FormattedText.Text.Should().Be("foobar");

			var results = new SearchResults();
			results.Add(new LogMatch(0, new LogLineMatch(1, 2)));
			textLine.SearchResults = results;
			var segments = textLine.Segments.ToList();
			segments.Count.Should().Be(3);
			segments[0].FormattedText.Text.Should().Be("f");
			segments[1].FormattedText.Text.Should().Be("oo");
			segments[2].FormattedText.Text.Should().Be("bar");
		}

		[Test]
		[Description("Verifies that a line is correctly split into different sections")]
		public void TestHighlight4()
		{
			var results = new SearchResults();
			results.Add(new LogMatch(0, new LogLineMatch(28, 4)));

			var textLine = new TextLine(new LogLine(0, 0, ".NET Environment: 4.0.30319.42000", LevelFlags.None), _hovered,
			                            _selected, true)
				{
					SearchResults = results
				};

			textLine.Segments.Count().Should().Be(3);
			textLine.Segments.ElementAt(0).Text.Should().Be(".NET Environment: 4.0.30319.");
			textLine.Segments.ElementAt(1).Text.Should().Be("4200");
			textLine.Segments.ElementAt(2).Text.Should().Be("0");
		}

		[Test]
		[Description("Verifies that a line is correctly split into different sections")]
		public void TestHighlight5()
		{
			var results = new SearchResults();
			results.Add(new LogMatch(0, new LogLineMatch(28, 5)));

			var textLine = new TextLine(new LogLine(0, 0, ".NET Environment: 4.0.30319.42000", LevelFlags.None), _hovered,
										_selected, true)
			{
				SearchResults = results
			};

			textLine.Segments.Count().Should().Be(2);
			textLine.Segments.ElementAt(0).Text.Should().Be(".NET Environment: 4.0.30319.");
			textLine.Segments.ElementAt(1).Text.Should().Be("42000");
		}

		[Test]
		public void TestForegroundBrush1()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.ErrorForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.ErrorForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);
		}

		[Test]
		public void TestForegroundBrush2()
		{
			_selected.Add(new LogLineIndex(0));

			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected, true);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);
		}

		[Test]
		public void TestBackgroundBrush1()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.WarningBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalBackgroundBrush);
		}

		[Test]
		public void TestBackgroundBrush2()
		{
			_hovered.Add(new LogLineIndex(0));

			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.WarningHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);
		}

		[Test]
		public void TestBackgroundBrush3()
		{
			_selected.Add(new LogLineIndex(0));

			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected, true);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);
		}
	}
}