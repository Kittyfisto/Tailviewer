using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Controls;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	public sealed class LogEntryListViewTest
	{
		private LogEntryListView _control;
		private Mock<ILogFile> _logFile;
		private List<LogLine> _lines;
		private List<ILogFileListener> _listeners;

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_control = new LogEntryListView
				{
					Width = 1024,
					Height = 768
				};
			var availableSize = new Size(1024, 768);
			_control.Measure(availableSize);
			_control.Arrange(new Rect(new Point(), availableSize));
			DispatcherExtensions.ExecuteAllEvents();

			_lines = new List<LogLine>();
			_listeners = new List<ILogFileListener>();

			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.Count).Returns(() => _lines.Count);
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
			        .Callback((LogFileSection section, LogLine[] dest) =>
			                  _lines.CopyTo((int) section.Index, dest, 0, section.Count));
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			        .Callback((ILogFileListener listener, TimeSpan maximumTimeout, int maximumLines) =>
				        {
					        _listeners.Add(listener);
					        listener.OnLogFileModified(_logFile.Object,
					                                   new LogFileSection(0, _lines.Count));
				        });
		}

		[Test]
		[STAThread]
		[Description("Verifies that an empty log file can be represented")]
		public void TestSetLogFile1()
		{
			new Action(() => _control.LogFile = _logFile.Object).ShouldNotThrow();
			_control.LogFile.Should().BeSameAs(_logFile.Object);

			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(0, "Because the log file is empty and thus no scrolling shall happen");
			_control.VerticalScrollBar.Value.Should().Be(0, "Because the log file is empty and thus no scrolling shall happen");
			_control.VerticalScrollBar.ViewportSize.Should().Be(751, "Because the viewport shall be as big as the control");
		}

		[Test]
		[STAThread]
		[Description("Verfies that a log file with one line can be represented")]
		public void TestSetLogFile2()
		{
			_lines.Add(new LogLine(0, 0, "Foobar", LevelFlags.Debug));

			new Action(() => _control.LogFile = _logFile.Object).ShouldNotThrow();

			_control.VisibleTextLines.Count.Should().Be(1, "Because the log file contains one log line");
			_control.VisibleTextLines[0].LogLine.Should().Be(_lines[0]);

			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(0, "Because the single line that is being displayed is less than the total of 48 that can be, hence no scrolling may be allowed");
			_control.VerticalScrollBar.Value.Should().Be(0, "Because there is less content than can be displayed and thus no scrolling is necessary");
			_control.VerticalScrollBar.ViewportSize.Should().Be(751, "Because the viewport shall be as big as the control");
		}

		[Test]
		[STAThread]
		[Description("Verfies that a log file with as many lines as the viewport can hold can be represented")]
		public void TestSetLogFile3()
		{
			_control.MaxNumFullyVisibleLines.Should().Be(46);
			for (int i = 0; i < 46; ++i)
			{
				_lines.Add(new LogLine(0, 0, "Foobar", LevelFlags.Debug));
			}

			new Action(() => _control.LogFile = _logFile.Object).ShouldNotThrow();

			_control.VisibleTextLines.Count.Should().Be(46, "Because the view can display 46 lines and we've added as many");
			for (int i = 0; i < 46; ++i)
			{
				_control.VisibleTextLines[i].LogLine.Should().Be(_lines[i]);
			}

			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(0, "Because we've added a total of 46 lines, which the view can display, and thus no scrolling should be necessary");
			_control.VerticalScrollBar.Value.Should().Be(0, "Because we've added a total of 46 lines, which the view can display, and thus no scrolling should be necessary");
			_control.VerticalScrollBar.ViewportSize.Should().Be(751, "Because the viewport shall be as big as the control minus the horizontal scrollbar");
		}

		[Test]
		[STAThread]
		[Description("Verfies that a log file with as one line more than the viewport can hold can be represented")]
		public void TestSetLogFile4()
		{
			_control.MaxNumVisibleLines.Should().Be(47);

			const int lineCount = 48;
			for (int i = 0; i < lineCount; ++i)
			{
				_lines.Add(new LogLine(0, 0, "Foobar", LevelFlags.Debug));
			}

			new Action(() => _control.LogFile = _logFile.Object).ShouldNotThrow();

			_control.VisibleTextLines.Count.Should().Be(47, "Because the view can display 47 of the 48 lines that we've added");
			for (int i = 0; i < 47; ++i)
			{
				_control.VisibleTextLines[i].LogLine.Should().Be(_lines[i]);
			}

			DispatcherExtensions.ExecuteAllEvents();

			_control.VerticalScrollBar.Minimum.Should().Be(0, "Because a scrollviewer should always start at 0");
			_control.VerticalScrollBar.Maximum.Should().Be(17, "Because we've added a total of 48 lines, which the view can display, and thus no scrolling should be necessary");
			_control.VerticalScrollBar.Value.Should().Be(0, "Because we've added a total of 48 lines, which the view can display, and thus no scrolling should be necessary");
			_control.VerticalScrollBar.ViewportSize.Should().Be(751, "Because the viewport shall be as big as the control");
		}

		[Test]
		[STAThread]
		[Description("Verifies that the view synchronizes itself with the log file when the latter was modified after being attached")]
		public void TestLogFileAdd1()
		{
			_control.LogFile = _logFile.Object;
			DispatcherExtensions.ExecuteAllEvents();

			for (int i = 0; i < 1000; ++i)
			{
				_lines.Add(new LogLine(i, i, "Foobar", LevelFlags.Info));
			}
			_listeners[0].OnLogFileModified(_logFile.Object, new LogFileSection(0, _lines.Count));

			_control.VisibleTextLines.Count.Should().Be(0, "Because the view may not have synchronized itself with the log file");
			_control.PendingModificationsCount.Should().Be(1, "Because this log file modification should have been tracked by the control");

			Thread.Sleep((int) (2*LogEntryListView.MaximumRefreshInterval.TotalMilliseconds));
			DispatcherExtensions.ExecuteAllEvents();

			_control.VisibleTextLines.Count.Should().Be(47, "Because the view must have synchronized itself and display the maximum of 48 lines");
		}

		[Test]
		[STAThread]
		[Description("Verifies that the ListView is capable of handling exceptions thrown by GetSection() that indicate that the log file has shrunk")]
		public void TestGetSectionThrows()
		{
			_control.LogFile = _logFile.Object;

			for (int i = 0; i < 1000; ++i)
			{
				_lines.Add(new LogLine(i, i, "Foobar", LevelFlags.Info));
			}
			_listeners[0].OnLogFileModified(_logFile.Object, new LogFileSection(0, _lines.Count));
		}
	}
}