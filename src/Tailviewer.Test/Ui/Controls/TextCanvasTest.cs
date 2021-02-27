using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Buffer;
using Tailviewer.Settings;
using Tailviewer.Test.BusinessLogic.Sources.Buffer;
using Tailviewer.Ui.LogView;
using WpfUnit;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class TextCanvasTest
	{
		private TestMouse _mouse;
		private TestKeyboard _keyboard;
		private TextCanvas _control;
		private IReadOnlyList<IColumnDescriptor> _columns;

		[SetUp]
		public void SetUp()
		{
			_mouse = new TestMouse();
			_keyboard = new TestKeyboard();
			_columns = GeneralColumns.Minimum.Concat(new[] {PageBufferedLogSource.RetrievalState}).ToList();

			_control = new TextCanvas(new ScrollBar(), new ScrollBar(), TextSettings.Default)
			{
				Width = 800,
				Height = 600
			};
			_control.Arrange(new Rect(0, 0, 800, 600));
			_control.ChangeTextSettings(new TextSettings(), new TextBrushes(null));
			DispatcherExtensions.ExecuteAllEvents();
		}

		[Test]
		[Description("Verifies that the control doesn't throw upon resizing when the current line is set to an impossible value, with regards to the log file")]
		public void TestOnSizeChanged1()
		{
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(0);
			_control.LogSource = logFile.Object;
			_control.CurrentLine = 1;

			new Action(() => _control.OnSizeChanged()).Should().NotThrow();

			_control.CurrentlyVisibleSection.Should().Equal(new LogFileSection(0, 0));
		}

		[Test]
		[Description("Verifies that the canvas compensates when the visible line is outside of the bounds of the source")]
		public void TestCalculateVisibleSection1()
		{
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(1);

			_control.LogSource = logFile.Object;
			_control.CurrentLine = 600;

			var section = _control.CalculateVisibleSection();
			section.Should().Equal(new LogFileSection(0, 1), "because the control should clamp the visible section until something better becomes available");
		}

		[Test]
		public void TestUpdateVisibleLine1()
		{
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(42);
			_control.LogSource = logFile.Object;

			logFile.Setup(x => x.GetEntries(It.IsAny<LogFileSection>(), It.IsAny<ILogBuffer>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>()))
				.Throws<IndexOutOfRangeException>();

			new Action(() => _control.UpdateVisibleLines()).Should().NotThrow();

			logFile.Setup(x => x.GetEntries(It.IsAny<LogFileSection>(), It.IsAny<ILogBuffer>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>()))
			       .Throws<ArgumentOutOfRangeException>();
			new Action(() => _control.UpdateVisibleLines()).Should().NotThrow();
		}

		[Test]
		public void TestSelectOneLine1()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);
			
			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			_control.SelectedIndices.Should().BeEmpty();

			_mouse.MoveRelativeTo(_control, new Point(20, 8));
			_control.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
			{
				RoutedEvent = UIElement.MouseLeftButtonDownEvent
			});

			_control.SelectedIndices.Should().Equal(new LogLineIndex(0));
		}

		[Test]
		[Description("Verifies that multiple lines can be selected by shift+mouse clicking")]
		public void TestSelectMultipleLines1()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);

			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			_control.SelectedIndices.Should().BeEmpty();

			_mouse.LeftClickAt(_control, new Point(20, 8));

			_control.SelectedIndices.Should().Equal(new LogLineIndex(0));

			_keyboard.Press(Key.LeftShift);
			_mouse.LeftClickAt(_control, new Point(20, 24));

			_control.SelectedIndices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1));
		}

		[Test]
		[Description("Verifies that a previous line is unselected when a different line is clicked")]
		public void TestSelectMultipleLines2()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);

			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			_control.SelectedIndices.Should().BeEmpty();

			_mouse.LeftClickAt(_control, new Point(20, 8));

			_control.SelectedIndices.Should().Equal(new LogLineIndex(0));

			_mouse.LeftClickAt(_control, new Point(20, 24));

			_control.SelectedIndices.Should().Equal(new LogLineIndex(1));
		}

		[Test]
		[Description("Verifies that multiple lines can be selected with shift+down")]
		public void TestSelectMultipleLinesWithKeyboard1()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);
			logFile.AddEntry("How's it going?", LevelFlags.Other);

			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			// This time we'll do the first selection programatically (which happens when
			// switching between data sources, for example)
			_control.SelectedIndices = new List<LogLineIndex> {new LogLineIndex(0)};
			_keyboard.Press(Key.LeftShift);
			_keyboard.Click(_control, Key.Down);

			_control.SelectedIndices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1));

			 _keyboard.Click(_control, Key.Down);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1), new LogLineIndex(2));

			_keyboard.Click(_control, Key.Up);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1));

			_keyboard.Click(_control, Key.Up);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(0));
		}

		[Test]
		[Description("Verifies that multiple lines can be selected with shift+up")]
		public void TestSelectMultipleLinesWithKeyboard2()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);
			logFile.AddEntry("How's", LevelFlags.Other);
			logFile.AddEntry("it", LevelFlags.Other);
			logFile.AddEntry("going?", LevelFlags.Other);

			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			// This time we'll do the first selection programatically (which happens when
			// switching between data sources, for example)
			_control.SelectedIndices = new List<LogLineIndex> { new LogLineIndex(2) };
			_keyboard.Press(Key.LeftShift);
			_keyboard.Click(_control, Key.Up);

			_control.SelectedIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(2));

			_keyboard.Click(_control, Key.Up);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1), new LogLineIndex(2));

			_keyboard.Click(_control, Key.Down);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(2));

			_keyboard.Click(_control, Key.Down);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(2));

			_keyboard.Click(_control, Key.Down);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(2), new LogLineIndex(3));

			_keyboard.Click(_control, Key.Down);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(2), new LogLineIndex(3), new LogLineIndex(4));
		}

		[Test]
		[Description("Verifies that keyboard shortcuts work when the user has started the selection with a mouse click")]
		public void TestSelectMultipleLinesWithKeyboard3()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);
			logFile.AddEntry("How's it going?", LevelFlags.Other);

			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			_mouse.LeftClickAt(_control, new Point(10, 24));
			_control.SelectedIndices.Should().Equal(new LogLineIndex(1));

			_keyboard.Press(Key.LeftShift);
			_keyboard.Click(_control, Key.Down);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(2));
		}

		[Test]
		[Description("Verifies that shortcuts work when the user tried to select more lines than are present")]
		public void TestSelectMultipleLinesWithKeyboard4()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);
			logFile.AddEntry("How's it going?", LevelFlags.Other);

			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			_mouse.LeftClickAt(_control, new Point(10, 40));
			_control.SelectedIndices.Should().Equal(new LogLineIndex(2));

			_keyboard.Press(Key.LeftShift);
			_keyboard.Click(_control, Key.Up);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(2));
			
			_keyboard.Click(_control, Key.Up);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1), new LogLineIndex(2));

			// We're now moving "beyond" the first item
			_keyboard.Click(_control, Key.Up);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1), new LogLineIndex(2));

			_keyboard.Click(_control, Key.Down);
			_control.SelectedIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(2));
		}

		[Test]
		[Description("Verifies that the control requests that the newly selected line is brought into view when using keyboard shortcuts")]
		public void TestSelectMultipleLinesWithKeyboard5()
		{
			var logFile = new InMemoryLogSource(_columns);
			logFile.AddEntry("Hello", LevelFlags.Other);
			logFile.AddEntry("World", LevelFlags.Other);
			logFile.AddEntry("How's it going?", LevelFlags.Other);

			_control.LogSource = logFile;
			_control.UpdateVisibleSection();
			_control.UpdateVisibleLines();

			_mouse.LeftClickAt(_control, new Point(10, 20));
			_control.SelectedIndices.Should().Equal(new LogLineIndex(1));

			var indices = new List<LogLineIndex>();
			_control.RequestBringIntoView += (index, match) => indices.Add(index);

			_keyboard.Press(Key.LeftShift);
			_keyboard.Click(_control, Key.Up);
			indices.Should().Equal(new object[] {new LogLineIndex(0)}, "because the newly selected log line should've been brought into view");

			indices.Clear();
			_keyboard.Click(_control, Key.Down);
			indices.Should().Equal(new object[] {new LogLineIndex(1)}, "because the newly selected log line should've been brought into view");
		}
	}
}