using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.LogView;
using Tailviewer.Ui.Controls.LogView.DeltaTimes;
using WpfUnit;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class LogEntryListViewTest
	{
		private LogEntryListView _control;
		private InMemoryLogFile _logFile;
		private TestKeyboard _keyboard;
		private TestMouse _mouse;
		private DeltaTimeColumnPresenter _deltaTimesColumn;

		[SetUp]
		public void SetUp()
		{
			_keyboard = new TestKeyboard();
			_mouse = new TestMouse();

			_control = new LogEntryListView
				{
					Width = 1024,
					Height = 768
				};
			var availableSize = new Size(1024, 768);
			_control.Measure(availableSize);
			_control.Arrange(new Rect(new Point(), availableSize));
			DispatcherExtensions.ExecuteAllEvents();


			_logFile = new InMemoryLogFile();

			_deltaTimesColumn = (DeltaTimeColumnPresenter)typeof(LogEntryListView).GetField("_deltaTimesColumn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_control);
		}

		[Test]
		public void TestCtor()
		{
			_control.LogFile.Should().BeNull();
			_control.FollowTail.Should().BeFalse();
		}

		[Test]
		[Description("Verifies that an empty log file can be represented")]
		public void TestSetLogFile1()
		{
			new Action(() => _control.LogFile = _logFile).Should().NotThrow();
			_control.LogFile.Should().BeSameAs(_logFile);

			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(0, "Because the log file is empty and thus no scrolling shall happen");
			_control.VerticalScrollBar.Value.Should().Be(0, "Because the log file is empty and thus no scrolling shall happen");
			_control.VerticalScrollBar.ViewportSize.Should().Be(768, "Because the viewport shall be as big as the control");
		}

		[Test]
		[Description("Verfies that a log file with one line can be represented")]
		public void TestSetLogFile2()
		{
			_logFile.AddEntry("Foobar", LevelFlags.Debug);

			new Action(() => _control.LogFile = _logFile).Should().NotThrow();

			_control.VisibleTextLines.Count.Should().Be(1, "Because the log file contains one log line");
			_control.VisibleTextLines[0].LogEntry.RawContent.Should().Be(_logFile[0].RawContent);
			_control.VisibleTextLines[0].LogEntry.LogLevel.Should().Be(_logFile[0].LogLevel);

			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(0, "Because the single line that is being displayed is less than the total of 48 that can be, hence no scrolling may be allowed");
			_control.VerticalScrollBar.Value.Should().Be(0, "Because there is less content than can be displayed and thus no scrolling is necessary");
			_control.VerticalScrollBar.ViewportSize.Should().Be(768, "Because the viewport shall be as big as the control");
		}

		[Test]
		[Description("Verfies that a log file with as many lines as the viewport can hold can be represented")]
		public void TestSetLogFile3()
		{
			for (int i = 0; i < 46; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Debug);
			}

			new Action(() => _control.LogFile = _logFile).Should().NotThrow();

			_control.VisibleTextLines.Count.Should().Be(46, "Because the view can display 46 lines and we've added as many");
			for (int i = 0; i < 46; ++i)
			{
				_control.VisibleTextLines[i].LogEntry.RawContent.Should().Be(_logFile[i].RawContent);
				_control.VisibleTextLines[i].LogEntry.LogLevel.Should().Be(_logFile[i].LogLevel);
			}

			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(0, "Because we've added a total of 46 lines, which the view can display, and thus no scrolling should be necessary");
			_control.VerticalScrollBar.Value.Should().Be(0, "Because we've added a total of 46 lines, which the view can display, and thus no scrolling should be necessary");
			_control.VerticalScrollBar.ViewportSize.Should().Be(768, "Because the viewport shall be as big as the control minus the horizontal scrollbar");
		}

		[Test]
		[Description("Verfies that a log file with as one line more than the viewport can hold can be represented")]
		public void TestSetLogFile4()
		{
			const int lineCount = 53;
			for (int i = 0; i < lineCount; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Debug);
			}

			new Action(() => _control.LogFile = _logFile).Should().NotThrow();

			// It takes some "time" until the control has done its layouting
			DispatcherExtensions.ExecuteAllEvents();

			_control.VisibleTextLines.Count.Should().Be(52, "Because the view can display 48 of the 49 lines that we've added");
			for (int i = 0; i < 52; ++i)
			{
				_control.VisibleTextLines[i].LogEntry.RawContent.Should().Be(_logFile[i].RawContent);
				_control.VisibleTextLines[i].LogEntry.LogLevel.Should().Be(_logFile[i].LogLevel);
			}

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(30, "Because we've added a total of 53 lines, of which the view can display 52 partially, and thus 30 pixels are missing in height");
			_control.VerticalScrollBar.Value.Should().Be(0);
			_control.VerticalScrollBar.ViewportSize.Should().Be(768, "Because the viewport shall be as big as the control");
		}

		[Test]
		[Description("Verfies that when a log file is set, its maximum number of characters for all lines is queried and used to calculate the maximum value of the horizontal scrollbar")]
		public void TestSetLogFile5()
		{
			_logFile.AddEntry(new string('f', 221), LevelFlags.Debug);
			new Action(() => _control.LogFile = _logFile).Should().NotThrow();

			_control.HorizontalScrollBar.Visibility.Should().Be(Visibility.Visible, "Because a scroll bar is necessary to view all contents of the log file");
			_control.HorizontalScrollBar.Maximum.Should().BeGreaterThan(0, "Because the log file claims that its greatest line contains more characters than can currently be represented by the control, hence a scrollbar is necessary");
		}

		[Test]
		[Description("Verifies that the view synchronizes itself with the log file when the latter was modified after being attached")]
		public void TestLogFileAdd1()
		{
			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
			_control.LogFile = _logFile;
			DispatcherExtensions.ExecuteAllEvents();

			var entries = new List<IReadOnlyLogEntry>();
			for (int i = 0; i < 1000; ++i)
			{
				entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
				{
					{Columns.RawContent, "Foobar" },
					{Columns.LogLevel, LevelFlags.Info }
				}));
			}
			_logFile.AddRange(entries);

			_control.VisibleTextLines.Count.Should().Be(0, "Because the view may not have synchronized itself with the log file");
			_control.PendingModificationsCount.Should().BeGreaterOrEqualTo(1, "Because this log file modification should have been tracked by the control");

			Thread.Sleep((int) (2*LogEntryListView.MaximumRefreshInterval.TotalMilliseconds));
			DispatcherExtensions.ExecuteAllEvents();

			_control.VisibleTextLines.Count.Should().Be(52, "Because the view must have synchronized itself and display the maximum of 52 lines");
		}

		[Test]
		[Ignore("Write proper test for it")]
		[Description("Verifies that the ListView is capable of handling exceptions thrown by GetSection() that indicate that the log file has shrunk")]
		public void TestGetSectionThrows()
		{
		}

		[Test]
		public void TestToggleShowDeltaTimes()
		{
			_control.ShowDeltaTimes.Should().BeFalse("because the delta times shoudn't be shown by default");
			_deltaTimesColumn.Visibility.Should().Be(Visibility.Collapsed, "because the column should be hidden from view");

			_control.ShowDeltaTimes = true;
			_deltaTimesColumn.Visibility.Should().Be(Visibility.Visible, "because the we've just configured this column to be visible");

			_control.ShowDeltaTimes = false;
			_deltaTimesColumn.Visibility.Should().Be(Visibility.Collapsed, "because the we've just hidden this column again");
		}

		[Test]
		[Description("Verifies that if the most recent log line becomes even partially obstructed, then the view is moved to make it fully visible when FollowTail is enabled")]
		public void TestFollowTail1()
		{
			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
			_control.LogFile = _logFile;
			DispatcherExtensions.ExecuteAllEvents();
			
			var entries = new List<IReadOnlyLogEntry>();
			for (int i = 0; i < 52; ++i)
			{
				entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
				{
					{Columns.RawContent, "Foobar" },
					{Columns.LogLevel, LevelFlags.Info }
				}));
			}
			_logFile.AddRange(entries);

			Thread.Sleep((int)(2 * LogEntryListView.MaximumRefreshInterval.TotalMilliseconds));
			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Value.Should().Be(0);
			_control.VisibleTextLines.Count.Should().Be(52);

			_control.FollowTail = true;
			_control.VerticalScrollBar.Maximum.Should().Be(15, "Because the view is missing 15 pixels to fully display the last row");
			_control.VerticalScrollBar.Value.Should().Be(15, "Because the vertical scrollbar should've moved in order to bring the last line *fully* into view");
		}

		[Test]
		[Description("Verifies that if FollowTail is enabled and lines are added to the log file, then the view scrolls to the bottom")]
		public void TestFollowTail2()
		{
			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
			_control.LogFile = _logFile;
			DispatcherExtensions.ExecuteAllEvents();
			
			var entries = new List<IReadOnlyLogEntry>();
			for (int i = 0; i < 51; ++i)
			{
				entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
				{
					{Columns.RawContent, "Foobar" },
					{Columns.LogLevel, LevelFlags.Info }
				}));
			}
			_logFile.AddRange(entries);

			Thread.Sleep((int)(2 * LogEntryListView.MaximumRefreshInterval.TotalMilliseconds));
			DispatcherExtensions.ExecuteAllEvents();


			_control.FollowTail = true;
			_logFile.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
			{
				{Columns.RawContent, "Foobar" },
				{Columns.LogLevel, LevelFlags.Info }
			}));
			_control.OnLogFileModified(_logFile, new LogFileSection(0, _logFile.Count));
			Thread.Sleep((int)(2 * LogEntryListView.MaximumRefreshInterval.TotalMilliseconds));
			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Maximum.Should().Be(15, "Because the view is missing 15 pixels to fully display the last row");
			_control.VerticalScrollBar.Value.Should().Be(15, "Because the vertical scrollbar should've moved in order to bring the last line *fully* into view");
		}

		[Test]
		[Description("Verifies that if the code in OnTimer throws an exception, then the exception is caught and another update is scheduled")]
		public void TestOnTimerException()
		{
			//_control.LogFile = _logFile;
			//_control.PendingModificationsCount.Should().Be(1, "Because the control should've queried an update due to attaching to the log file");

			//bool insideTest = true;
			//bool exceptionThrown = false;
			//_logFile.Setup(x => x.Count).Callback(() =>
			//	{
			//		exceptionThrown = true;

			//		if (insideTest)
			//			throw new SystemException();
			//	});

			//new Action(() => _control.OnTimer(null, null)).Should().NotThrow("Because any and all exceptions must be handled inside this callback");
			//_control.PendingModificationsCount.Should().Be(1, "Because another update should've been scheduled as this one wasn't fully completed");
			//exceptionThrown.Should().BeTrue("Because the control should've queried the ILogFile.Count property during its update");

			//insideTest = false;
		}

		[Ignore("Not finished yet")]
		[Test]
		[Description("Verifies a mouse left down selects the item under it")]
		public void TestSelect1()
		{
			_logFile.AddEntry("Foobar", LevelFlags.Info);
			_logFile.AddEntry("Foobar", LevelFlags.Info);
			_control.LogFile = _logFile;
			_control.SelectedIndices.Should().BeEmpty();

			_control.SelectedIndices.Should().Equal(new[]
				{
					new LogLineIndex(1)
				});
		}

		[Test]
		[Description("Verifies that only selected lines are copied to the clipboard")]
		public void TestCopyToClipboard1()
		{
			_logFile.AddEntry("Foobar", LevelFlags.Info);
			_logFile.AddEntry("Clondyke bar", LevelFlags.Info);
			_control.LogFile = _logFile;
			_control.Select(1);
			_control.CopySelectedLinesToClipboard();

			Clipboard.GetText().Should().Be("Clondyke bar");
		}

		[Test]
		[Description("Verifies that multiple lines can be copied to the clipboard")]
		public void TestCopyToClipboard2()
		{
			_logFile.AddEntry("Foobar", LevelFlags.Info);
			_logFile.AddEntry("Clondyke bar", LevelFlags.Info);
			_control.LogFile = _logFile;
			_control.Select(1, 0);
			_control.CopySelectedLinesToClipboard();

			Clipboard.GetText().Should().Be("Foobar\r\nClondyke bar");
		}

		[Test]
		[Description("Verifies that when the mouse wheel is used to scroll to the last line, then FollowTail is automatically enabled")]
		public void TestMouseWheelDown1()
		{
			for (int i = 0; i < 51; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;

			_control.VerticalScrollBar.Value.Should().BeLessThan(_control.VerticalScrollBar.Maximum);
			_control.FollowTail.Should().BeFalse();

			_mouse.RotateMouseWheelDown(_control.PartTextCanvas);
			_control.VerticalScrollBar.Value.Should().Be(_control.VerticalScrollBar.Maximum);
			_control.FollowTail.Should().BeTrue("because scrolling down to the last line shall automatically enable follow tail");
		}

		[Test]
		[Description("Verifies that if no settings are present, then the default scroll speed is used")]
		public void TestMouseWheelDown2()
		{
			for (int i = 0; i < 100; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;

			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should().Be(0, "because the control should display from the first line onwards");
			_control.Settings = null;
			_mouse.RotateMouseWheelDown(_control.PartTextCanvas);
			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should()
			        .Be(2, "because we should've scrolled down by the default amount of lines");
		}

		[Test]
		[Description("Verifies that the control scrolls by the configured amount of lines")]
		public void TestMouseWheelDown3([Values(1, 2, 3, 4, 5)] int linesToScroll)
		{
			for (int i = 0; i < 100; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;

			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should().Be(0, "because the control should display from the first line onwards");
			_control.Settings = new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesToScroll
			};
			_mouse.RotateMouseWheelDown(_control.PartTextCanvas);
			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should()
			        .Be(linesToScroll, "because we should've scrolled down by the configured amount of lines");
		}

		[Test]
		[Description("Verifies that when the mouse wheel is used to scroll up from the last line, then FollowTail is automatically disabled")]
		public void TestMouseWheelUp1()
		{
			for (int i = 0; i < 51; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;

			_mouse.RotateMouseWheelDown(_control.PartTextCanvas);
			_control.VerticalScrollBar.Value.Should().Be(_control.VerticalScrollBar.Maximum);
			_control.FollowTail.Should().BeTrue();

			_mouse.RotateMouseWheelUp(_control.PartTextCanvas);
			_control.VerticalScrollBar.Value.Should().BeLessThan(_control.VerticalScrollBar.Maximum);
			_control.FollowTail.Should().BeFalse("because scrolling up shall automatically disable follow tail");
		}

		[Test]
		[Description("Verifies that the control scrolls by the default amount of lines when no settings are available")]
		public void TestMouseWheelUp2()
		{
			for (int i = 0; i < 100; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;
			_keyboard.Press(Key.LeftCtrl);
			_keyboard.Click(_control.PartTextCanvas, Key.End);

			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should().Be(50, "because the control should display the last lines of the log file");
			_control.PartTextCanvas.CurrentlyVisibleSection.Count.Should().Be(50, "because the control should display the last lines of the log file");
			_control.Settings = null;
			_mouse.RotateMouseWheelUp(_control.PartTextCanvas);
			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should()
			        .Be(48, "because we should've scrolled up by the default amount of lines");
		}

		[Test]
		[Description("Verifies that the control scrolls by the configured amount of lines")]
		public void TestMouseWheelUp3([Values(1, 2, 3, 4, 5)] int linesToScroll)
		{
			for (int i = 0; i < 100; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;
			_keyboard.Press(Key.LeftCtrl);
			_keyboard.Click(_control.PartTextCanvas, Key.End);

			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should().Be(50, "because the control should display the last lines of the log file");
			_control.PartTextCanvas.CurrentlyVisibleSection.Count.Should().Be(50, "because the control should display the last lines of the log file");
			_control.Settings = new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesToScroll
			};
			_mouse.RotateMouseWheelUp(_control.PartTextCanvas);
			_control.PartTextCanvas.CurrentlyVisibleSection.Index.Should()
			        .Be(50 - linesToScroll, "because we should've scrolled up by the configured amount of lines");
		}

		[Test]
		[Description("Verifies that when control+end is pressed, then the last line is both brought into view and selected")]
		public void TestControlEnd()
		{
			for (int i = 0; i < 200; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;

			_control.SelectedIndices.Should().BeEmpty();
			_control.FollowTail.Should().BeFalse();

			_keyboard.Press(Key.LeftCtrl);
			_keyboard.Click(_control.PartTextCanvas, Key.End);
			_control.SelectedIndices.Should().Equal(new object[] {new LogLineIndex(199)});
			_control.PartTextCanvas.CurrentlyVisibleSection.Should().Equal(new LogFileSection(150, 50));
			_control.FollowTail.Should().BeTrue("because scrolling down to the last line shall automatically enable follow tail");
		}

		[Test]
		[Description("Verifies that when control+start is pressed, then the first line is both brought into view and selected")]
		public void TestControlStart()
		{
			for (int i = 0; i < 200; ++i)
			{
				_logFile.AddEntry("Foobar", LevelFlags.Info);
			}
			_control.LogFile = _logFile;

			_keyboard.Press(Key.LeftCtrl);
			_keyboard.Click(_control.PartTextCanvas, Key.End);
			_control.SelectedIndices.Should().Equal(new object[] { new LogLineIndex(199) });
			_control.PartTextCanvas.CurrentlyVisibleSection.Should().Equal(new LogFileSection(150, 50));
			_control.FollowTail.Should().BeTrue();

			_keyboard.Click(_control.PartTextCanvas, Key.Home);
			_control.SelectedIndices.Should().Equal(new object[] { new LogLineIndex(0) });
			_control.PartTextCanvas.CurrentlyVisibleSection.Should().Equal(new LogFileSection(0, 50));
			_control.FollowTail.Should().BeFalse();
		}
	}
}