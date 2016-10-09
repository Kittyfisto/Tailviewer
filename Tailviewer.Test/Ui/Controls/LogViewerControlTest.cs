using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	public sealed class LogViewerControlTest
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_dataSource = new SingleDataSourceViewModel(new SingleDataSource(_scheduler, new DataSource("Foobar") {Id = Guid.NewGuid()}));
			_control = new LogViewerControl
				{
					DataSource = _dataSource,
					Width = 1024,
					Height = 768
				};

			DispatcherExtensions.ExecuteAllEvents();

			_dispatcher = new ManualDispatcher();
		}

		private LogViewerControl _control;
		private ManualDispatcher _dispatcher;
		private SingleDataSourceViewModel _dataSource;
		private ManualTaskScheduler _scheduler;

		[Test]
		[STAThread]
		[Ignore("Doesn't work yet")]
		[Description(
			"Verifies that upon setting the data source, the FollowTail property is forwarded to the LogEntryListView")]
		public void TestChangeDataSource1()
		{
			var dataSource = new Mock<IDataSourceViewModel>();
			dataSource.Setup(x => x.FollowTail).Returns(true);

			_control.DataSource = dataSource.Object;
			_control.PartListView.FollowTail.Should().BeTrue();
		}

		[Test]
		[STAThread]
		[Description("Verifies that the ShowLineNumbers value on the new data source is used")]
		public void TestChangeDataSource2()
		{
			var dataSource = new Mock<IDataSourceViewModel>();
			dataSource.Setup(x => x.ShowLineNumbers).Returns(false);

			_control.ShowLineNumbers = true;
			_control.DataSource = dataSource.Object;
			_control.ShowLineNumbers.Should().BeFalse();
		}

		[Test]
		[STAThread]
		[Description("Verifies that the ShowLineNumbers value on the new data source is used")]
		public void TestChangeDataSource3()
		{
			var dataSource = new Mock<IDataSourceViewModel>();
			dataSource.Setup(x => x.ShowLineNumbers).Returns(true);

			_control.ShowLineNumbers = false;
			_control.DataSource = dataSource.Object;
			_control.ShowLineNumbers.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelAll()
		{
			_control.DataSource.LevelsFilter = LevelFlags.All;
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelDebug()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Debug;
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelError()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Error;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelFatal()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Fatal;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelInfo()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Info;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelWarning()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Warning;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		[Description("Verifies that changing the LogView does NOT change the currently visible line of the old view")]
		public void TestChangeLogView1()
		{
			// TODO: This test requires that the template be fully loaded (or the control changed to a user control)

			var oldLog = new Mock<IDataSourceViewModel>();
			var oldDataSource = new Mock<IDataSource>();
			var oldLogFile = new Mock<ILogFile>();
			oldLogFile.Setup(x => x.Count).Returns(10000);
			oldDataSource.Setup(x => x.FilteredLogFile).Returns(oldLogFile.Object);
			oldDataSource.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			oldLog.Setup(x => x.DataSource).Returns(oldDataSource.Object);
			oldLog.SetupProperty(x => x.VisibleLogLine);
			oldLog.Object.VisibleLogLine = 42;
			var oldLogView = new LogViewerViewModel(oldLog.Object, _dispatcher);
			_control.LogView = oldLogView;


			var newLog = new Mock<IDataSourceViewModel>();
			var newDataSource = new Mock<IDataSource>();
			newDataSource.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			newDataSource.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			newLog.Setup(x => x.DataSource).Returns(newDataSource.Object);
			newLog.SetupProperty(x => x.VisibleLogLine);
			newLog.Object.VisibleLogLine = 1;
			var newLogView = new LogViewerViewModel(newLog.Object, _dispatcher);
			_control.LogView = newLogView;

			oldLog.Object.VisibleLogLine.Should()
			      .Be(42, "Because the control shouldn't have changed the VisibleLogLine of the old logview");
			newLog.Object.VisibleLogLine.Should()
			      .Be(1, "Because the control shouldn't have changed the VisibleLogLine of the old logview");
		}

		[Test]
		[STAThread]
		[Description(
			"Verifies that the VisibleLogLine of a data source is properly propagated through all controls when the data source is changed"
			)]
		public void TestChangeLogView2()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Count).Returns(100);
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(logFile.Object);
			var dataSourceViewModel = new Mock<IDataSourceViewModel>();
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			dataSourceViewModel.Setup(x => x.VisibleLogLine).Returns(new LogLineIndex(42));
			var logView = new LogViewerViewModel(dataSourceViewModel.Object, _dispatcher);

			_control.LogView = logView;
			_control.CurrentLogLine.Should().Be(42);
			_control.PartListView.CurrentLine.Should().Be(42);
			_control.PartListView.PartTextCanvas.CurrentLine.Should().Be(42);
		}

		[Test]
		[STAThread]
		[Description("Verifies that when a new data source is attached, its Selection is used")]
		public void TestChangeLogView3()
		{
			_control.DataSource = null;

			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Count).Returns(100);
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(logFile.Object);
			var dataSourceViewModel = new Mock<IDataSourceViewModel>();
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			dataSourceViewModel.Setup(x => x.SelectedLogLines).Returns(new HashSet<LogLineIndex> {new LogLineIndex(42)});
			var logView = new LogViewerViewModel(dataSourceViewModel.Object, _dispatcher);

			_control.LogView = logView;
			_control.SelectedIndices.Should().Equal(new[] {new LogLineIndex(42)});
		}

		[Test]
		[STAThread]
		[Description(
			"Verifies that when a new data source is attached, the string filter of the new source is immediately used for highlighting"
			)]
		public void TestChangeLogView4()
		{
			_control.DataSource = null;

			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Count).Returns(100);
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(logFile.Object);
			var dataSourceViewModel = new Mock<IDataSourceViewModel>();
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			dataSourceViewModel.Setup(x => x.SelectedLogLines).Returns(new HashSet<LogLineIndex> {new LogLineIndex(42)});
			dataSourceViewModel.Setup(x => x.SearchTerm).Returns("Foobar");
			var logView = new LogViewerViewModel(dataSourceViewModel.Object, _dispatcher);

			_control.PART_SearchBox.Text.Should().Be(string.Empty);

			_control.LogView = logView;
			_control.SelectedIndices.Should().Equal(new[] {new LogLineIndex(42)});
			_control.PART_SearchBox.Text.Should().Be("Foobar");
		}

		[Test]
		[STAThread]
		public void TestChangeSelection1()
		{
			_dataSource.SelectedLogLines.Should().BeEmpty();
			_control.Select(new LogLineIndex(1));
			_dataSource.SelectedLogLines.Should().Equal(new[] {new LogLineIndex(1)});
		}

		[Test]
		[STAThread]
		public void TestChangeShowDebug()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowDebug.Should().BeFalse();

			_control.ShowDebug = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Debug);

			_control.ShowDebug = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowError()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowError.Should().BeFalse();

			_control.ShowError = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Error);

			_control.ShowError = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowFatal()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowFatal.Should().BeFalse();

			_control.ShowFatal = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Fatal);

			_control.ShowFatal = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowInfo()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowInfo.Should().BeFalse();

			_control.ShowInfo = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Info);

			_control.ShowInfo = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowWarning()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowWarning.Should().BeFalse();

			_control.ShowWarning = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Warning);

			_control.ShowWarning = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestCtor()
		{
			var source = new SingleDataSourceViewModel(new SingleDataSource(_scheduler, new DataSource("Foobar") {Id = Guid.NewGuid()}));
			source.LevelsFilter = LevelFlags.All;

			var control = new LogViewerControl
				{
					DataSource = source
				};
			control.ShowDebug.Should().BeTrue();
			control.ShowInfo.Should().BeTrue();
			control.ShowWarning.Should().BeTrue();
			control.ShowError.Should().BeTrue();
			control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestShowAll1()
		{
			_control.ShowAll = true;
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestShowAll2()
		{
			_control.ShowDebug = true;
			_control.ShowInfo = true;
			_control.ShowWarning = true;
			_control.ShowError = true;
			_control.ShowFatal = true;
			_control.ShowAll = false;

			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestShowAll3()
		{
			_control.ShowAll = true;
			_control.ShowDebug = false;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestShowAll4()
		{
			_control.ShowAll = false;
			_control.ShowDebug = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestShowAll5()
		{
			_control.ShowAll = false;
			_control.ShowDebug = true;
			_control.ShowInfo = true;
			_control.ShowWarning = true;
			_control.ShowError = true;
			_control.ShowFatal = true;
			_control.ShowAll.Should().BeTrue();
		}
	}
}