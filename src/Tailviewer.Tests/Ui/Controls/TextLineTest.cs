using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows.Media;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Tests.Ui.Controls
{
	[TestFixture]
	public sealed class TextLineTest
	{
		private HashSet<LogLineIndex> _hovered;
		private HashSet<LogLineIndex> _selected;

		private static IReadOnlyLogEntry CreateLogEntry(int lineIndex, int logEntryIndex, string message, LevelFlags flags)
		{
			return new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
			{
				{Columns.Index, (LogLineIndex)lineIndex },
				{Columns.LogEntryIndex, (LogEntryIndex)logEntryIndex },
				{Columns.RawContent, message },
				{Columns.LogLevel, flags }
			});
		}

		[SetUp]
		public void SetUp()
		{
			_hovered = new HashSet<LogLineIndex>();
			_selected = new HashSet<LogLineIndex>();
		}

		[Test]
		public void TestColorByLevel1()
		{
			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, false);
			textLine.ColorByLevel.Should().BeFalse();
		}

		[Test]
		public void TestColorByLevel2()
		{
			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.ColorByLevel.Should().BeTrue();
		}

		[Test]
		public void TestColorByLevel3()
		{
			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			textLine.ColorByLevel = false;
			textLine.ColorByLevel.Should().BeFalse();
		}

		[Test]
		public void TestHighlight1()
		{
			var results = new SearchResults();
			results.Add(new LogMatch(0, new LogLineMatch(1, 2)));

			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true)
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
			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			var segments = textLine.Segments.ToList();
			segments.Count.Should().Be(1);
			segments[0].FormattedText.Text.Should().Be("foobar");
		}

		[Test]
		[Description("Verifies that changing the filter on a TextLine results in segments changing")]
		public void TestHighlight3()
		{
			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
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

			var textLine = new TextLine(CreateLogEntry(0, 0, ".NET Environment: 4.0.30319.42000", LevelFlags.Other), _hovered,
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

			var textLine = new TextLine(CreateLogEntry(0, 0, ".NET Environment: 4.0.30319.42000", LevelFlags.Other), _hovered,
										_selected, true)
			{
				SearchResults = results
			};

			textLine.Segments.Count.Should().Be(2);
			textLine.Segments.ElementAt(0).Text.Should().Be(".NET Environment: 4.0.30319.");
			textLine.Segments.ElementAt(1).Text.Should().Be("42000");
		}

		[Test]
		[Description("Verifies that highlighting works correct on lines containing tabs")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/244")]
		public void TestHighlightTabs()
		{
			var results = new SearchResults();
			var message = "		double tabs, offset by eight";
			var searchTerm = "tabs";
			results.Add(new LogMatch(0, new LogLineMatch(message.IndexOf(searchTerm), searchTerm.Length)));
			const int tabWidth = 4; //< It's important that we run this test with a tabwidth of > 1
			var textSettings = new TextSettings(tabWidth: tabWidth);

			var textLine = new TextLine(CreateLogEntry(0, 0, message, LevelFlags.Other), _hovered,
			                            _selected, true, textSettings, new TextBrushes(null))
			{
				SearchResults = results
			};

			textLine.Segments.Count.Should().Be(3);
			textLine.Segments.ElementAt(0).Text.Should().Be("        double ");
			textLine.Segments.ElementAt(1).Text.Should().Be("tabs");
			textLine.Segments.ElementAt(2).Text.Should().Be(", offset by eight");
		}

		[Test]
		public void TestForegroundBrush1()
		{
			var textSettings = new TextSettings();
			var textBrushes = new TextBrushes(new LogViewerSettings());

			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.ForegroundBrush).Should().Be(LogViewerSettings.DefaultFatal.ForegroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.ForegroundBrush).Should().Be(LogViewerSettings.DefaultError.ForegroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.ForegroundBrush).Should().Be(LogViewerSettings.DefaultWarning.ForegroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.ForegroundBrush).Should().Be(LogViewerSettings.DefaultInfo.ForegroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.ForegroundBrush).Should().Be(LogViewerSettings.DefaultDebug.ForegroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Trace), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.ForegroundBrush).Should().Be(LogViewerSettings.DefaultTrace.ForegroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Other), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.ForegroundBrush).Should().Be(LogViewerSettings.DefaultInfo.ForegroundColor);
		}

		[Test]
		public void TestForegroundBrush2()
		{
			var textSettings = new TextSettings();
			var textBrushes = new TextBrushes(new LogViewerSettings());

			_selected.Add(new LogLineIndex(0));

			foreach (LevelFlags level in Enum.GetValues(typeof(LevelFlags)))
			{
				var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", level), _hovered, _selected, true, textSettings, textBrushes);
				textLine.ForegroundBrush.Should().Be(TextBrushes.SelectedForegroundBrush);
			}
		}

		[Test]
		public void TestBackgroundBrush1()
		{
			var textSettings = new TextSettings();
			var textBrushes = new TextBrushes(new LogViewerSettings());

			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.BackgroundBrush).Should().Be(LogViewerSettings.DefaultFatal.BackgroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.BackgroundBrush).Should().Be(LogViewerSettings.DefaultError.BackgroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.BackgroundBrush).Should().Be(LogViewerSettings.DefaultWarning.BackgroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.BackgroundBrush).Should().Be(LogViewerSettings.DefaultInfo.BackgroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.BackgroundBrush).Should().Be(LogViewerSettings.DefaultDebug.BackgroundColor);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Other), _hovered, _selected, true, textSettings, textBrushes);
			BrushColor(textLine.BackgroundBrush).Should().Be(LogViewerSettings.DefaultTrace.BackgroundColor);
		}
		/*
		[Test]
		public void TestBackgroundBrush2()
		{
			var textSettings = new TextSettings();
			var textBrushes = new TextBrushes(new LogViewerSettings());

			_hovered.Add(new LogLineIndex(0));

			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true, textSettings, textBrushes);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorHighlightBackgroundBrush);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Error), _hovered, _selected, true, textSettings, textBrushes);
			textLine.BackgroundBrush.Should().Be(TextHelper.ErrorHighlightBackgroundBrush);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Warning), _hovered, _selected, true, textSettings, textBrushes);
			textLine.BackgroundBrush.Should().Be(TextHelper.WarningHighlightBackgroundBrush);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Info), _hovered, _selected, true, textSettings, textBrushes);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Debug), _hovered, _selected, true, textSettings, textBrushes);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);

			textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Other), _hovered, _selected, true, textSettings, textBrushes);
			textLine.BackgroundBrush.Should().Be(TextHelper.NormalHighlightBackgroundBrush);
		}*/

		[Test]
		public void TestBackgroundBrush3()
		{
			var textSettings = new TextSettings();
			var textBrushes = new TextBrushes(new LogViewerSettings());

			_selected.Add(new LogLineIndex(0));
			
			foreach (LevelFlags level in Enum.GetValues(typeof(LevelFlags)))
			{
				var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", level), _hovered, _selected, true, textSettings, textBrushes);
				textLine.BackgroundBrush.Should().Be(TextBrushes.SelectedBackgroundBrush);
			}
		}

		[Test]
		[Description("Verifies that no exception is thrown when a 'bad' search result was specified")]
		public void TestIncorrectSearchResult()
		{
			var textLine = new TextLine(CreateLogEntry(0, 0, "foobar", LevelFlags.Fatal), _hovered, _selected, true);
			var searchResults = new SearchResults();
			searchResults.Add(new LogLineIndex(0), new LogLineMatch(42, 101));
			
			new Action(() => textLine.SearchResults = searchResults).Should().NotThrow();

			IEnumerable<TextSegment> segments = null;
			new Action(() => segments = textLine.Segments).Should().NotThrow();

			segments.Should().NotBeEmpty();
			segments.Count().Should().Be(1);
			segments.ElementAt(0).Text.Should().Be("foobar", "because if, for some reason, highlighting doesn't work, then the original, non-highlithed, line should be displayed");
		}

		[Test]
		[Description("Verifies that TextLine can deal with a completely empty logline")]
		public void TestGetSegments1()
		{
			var textLine = new TextLine(CreateLogEntry(0, 0, null, LevelFlags.None), _hovered, _selected, true);
			new Action(() =>
			{
				var unused = textLine.Segments;
			}).Should().NotThrow();
			var segments = textLine.Segments;
			segments.Should().BeEmpty();
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/165")]
		public void TestLongLogLine()
		{
			var message = new StringBuilder();
			message.Append('a', 10000);
			var textLine = new TextLine(CreateLogEntry(1, 1, message.ToString(), LevelFlags.Other), _hovered, _selected, true);
			textLine.Segments.Count.Should().BeGreaterThan(1, "because this very long line should've been split up into multiple messages");
		}

		[Test]
		public void TestReplaceTabsWithSpaces()
		{
			string replace(string input, int tabWidth)
			{
				var builder = new StringBuilder(input);
				TextLine.ReplaceTabsWithSpaces(builder, tabWidth);
				return builder.ToString();
			}

			replace("\t", 4).Should().Be("    ");
			replace(" \t", 4).Should().Be("    ");
			replace("a\t", 4).Should().Be("a   ");
			replace("ab\t", 4).Should().Be("ab  ");
			replace("abc\t", 4).Should().Be("abc ");
			replace("abcd\t", 4).Should().Be("abcd    ");
			replace("a\t\t", 2).Should().Be("a   ");
		}

		[Pure]
		private static Color BrushColor(Brush brush)
		{
			brush.Should().BeOfType<SolidColorBrush>();
			return ((SolidColorBrush) brush).Color;
		}
	}
}