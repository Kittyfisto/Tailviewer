using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
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
		public void TestHighlight2()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected);
			var segments = textLine.Segments.ToList();
			segments.Count.Should().Be(1);
			segments[0].FormattedText.Text.Should().Be("foobar");
		}

		[Test]
		public void TestHighlight1()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected)
				{
					Filter = Filter.Create("oo")
				};
			var segments = textLine.Segments.ToList();
			segments.Count.Should().Be(3);
			segments[0].FormattedText.Text.Should().Be("f");
			segments[1].FormattedText.Text.Should().Be("oo");
			segments[2].FormattedText.Text.Should().Be("bar");
		}

		[Test]
		[Description("Verifies that changing the filter on a TextLine results in segments changing")]
		public void TestHighlight3()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected);
			textLine.Segments.Count().Should().Be(1);
			textLine.Segments.First().FormattedText.Text.Should().Be("foobar");

			textLine.Filter = Filter.Create("oo");
			var segments = textLine.Segments.ToList();
			segments.Count.Should().Be(3);
			segments[0].FormattedText.Text.Should().Be("f");
			segments[1].FormattedText.Text.Should().Be("oo");
			segments[2].FormattedText.Text.Should().Be("bar");
		}

		[Test]
		public void TestForegroundBrush1()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.ErrorForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.ErrorForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.NormalForegroundBrush);
		}

		[Test]
		public void TestForegroundBrush2()
		{
			_selected.Add(new LogLineIndex(0));

			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected);
			textLine.ForegroundBrush.Should().Be(TextHelper.SelectedForegroundBrush);
		}

		[Test]
		public void TestBackgroundBrush1()
		{
			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.WarningBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalBackgroundBrush);
		}

		[Test]
		public void TestBackgroundBrush2()
		{
			_hovered.Add(new LogLineIndex(0));

			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.WarningHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);
		}

		[Test]
		public void TestBackgroundBrush3()
		{
			_selected.Add(new LogLineIndex(0));

			var textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Error), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Info), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);

			textLine = new TextLine(new LogLine(0, 0, "foobar", LevelFlags.None), _hovered, _selected);
			textLine.BackgroundBrush.Should().Be(TextHelper.SelectedBackgroundBrush);
		}
	}
}