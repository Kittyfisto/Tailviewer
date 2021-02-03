using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.LogView;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class LogViewerControlTest
	{
		private Mock<IActionCenter> _actionCenter;
		private LogViewerControl _control;
		private FileDataSourceViewModel _dataSource;
		private ILogFileFactory _logFileFactory;
		private ManualTaskScheduler _scheduler;
		private Mock<IApplicationSettings> _settings;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
			_actionCenter = new Mock<IActionCenter>();
		}

		[SetUp]
		public void SetUp()
		{
			_settings = new Mock<IApplicationSettings>();
			_dataSource =
				new FileDataSourceViewModel(
					new FileDataSource(_logFileFactory, _scheduler, new DataSource("Foobar") {Id = DataSourceId.CreateNew()}),
					_actionCenter.Object);
			_control = new LogViewerControl
			{
				DataSource = _dataSource,
				Width = 1024,
				Height = 768
			};

			DispatcherExtensions.ExecuteAllEvents();
		}

		[Test]
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
		public void TestChangeLevelAll()
		{
			_control.DataSource.LevelsFilter = LevelFlags.All;
			_control.ShowTrace.Should().BeTrue();
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		public void TestChangeLevelDebug()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Debug;
			_control.ShowTrace.Should().BeFalse();
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		public void TestChangeLevelError()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Error;
			_control.ShowTrace.Should().BeFalse();
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		public void TestChangeLevelFatal()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Fatal;
			_control.ShowTrace.Should().BeFalse();
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		public void TestChangeLevelInfo()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Info;
			_control.ShowTrace.Should().BeFalse();
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		public void TestChangeLevelWarning()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Warning;
			_control.ShowTrace.Should().BeFalse();
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		public void TestChangeLevelTrace()
		{
			_control.DataSource.LevelsFilter = LevelFlags.Trace;
			_control.ShowTrace.Should().BeTrue();
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
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
			var oldLogView = new LogViewerViewModel(oldLog.Object, _actionCenter.Object, _settings.Object);

			_control.LogView = oldLogView;


			var newLog = new Mock<IDataSourceViewModel>();
			var newDataSource = new Mock<IDataSource>();
			newDataSource.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			newDataSource.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			newLog.Setup(x => x.DataSource).Returns(newDataSource.Object);
			newLog.SetupProperty(x => x.VisibleLogLine);
			newLog.Object.VisibleLogLine = 1;
			var newLogView = new LogViewerViewModel(newLog.Object, _actionCenter.Object, _settings.Object);
			_control.LogView = newLogView;

			oldLog.Object.VisibleLogLine.Should()
				.Be(42, "Because the control shouldn't have changed the VisibleLogLine of the old logview");
			newLog.Object.VisibleLogLine.Should()
				.Be(1, "Because the control shouldn't have changed the VisibleLogLine of the old logview");
		}

		[Test]
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
			var logView = new LogViewerViewModel(dataSourceViewModel.Object, _actionCenter.Object, _settings.Object);

			_control.LogView = logView;
			_control.CurrentLogLine.Should().Be(42);
			_control.PartListView.CurrentLine.Should().Be(42);
			_control.PartListView.PartTextCanvas.CurrentLine.Should().Be(42);
		}

		[Test]
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
			var logView = new LogViewerViewModel(dataSourceViewModel.Object, _actionCenter.Object, _settings.Object);

			_control.LogView = logView;
			_control.SelectedIndices.Should().Equal(new LogLineIndex(42));
		}

		[Test]
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
			var logView = new LogViewerViewModel(dataSourceViewModel.Object, _actionCenter.Object, _settings.Object);

			_control.SearchBox.Text.Should().Be(string.Empty);

			_control.LogView = logView;
			_control.SelectedIndices.Should().Equal(new LogLineIndex(42));
			_control.SearchBox.Text.Should().Be("Foobar");
		}

		[Test]
		public void TestChangeSelection1()
		{
			_dataSource.SelectedLogLines.Should().BeEmpty();
			_control.Select(new LogLineIndex(1));
			_dataSource.SelectedLogLines.Should().Equal(new LogLineIndex(1));
		}

		[Test]
		public void TestChangeShowOther()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowOther.Should().BeFalse();

			_control.ShowOther = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Other);

			_control.ShowOther = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
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
		public void TestCtor()
		{
			var source =
				new FileDataSourceViewModel(
					new FileDataSource(_logFileFactory, _scheduler, new DataSource("Foobar") {Id = DataSourceId.CreateNew()}),
					_actionCenter.Object);
			source.LevelsFilter = LevelFlags.All;

			var control = new LogViewerControl
			{
				DataSource = source
			};
			control.ShowOther.Should().BeTrue();
			control.ShowTrace.Should().BeTrue();
			control.ShowDebug.Should().BeTrue();
			control.ShowInfo.Should().BeTrue();
			control.ShowWarning.Should().BeTrue();
			control.ShowError.Should().BeTrue();
			control.ShowFatal.Should().BeTrue();
		}

		[Test]
		public void TestShowAll1()
		{
			_control.ShowAll = true;
			_control.ShowOther.Should().BeTrue();
			_control.ShowTrace.Should().BeTrue();
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		public void TestShowAll2()
		{
			_control.ShowOther = true;
			_control.ShowTrace = true;
			_control.ShowDebug = true;
			_control.ShowInfo = true;
			_control.ShowWarning = true;
			_control.ShowError = true;
			_control.ShowFatal = true;
			_control.ShowAll = false;

			_control.ShowOther.Should().BeFalse();
			_control.ShowTrace.Should().BeFalse();
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		public void TestShowAll3()
		{
			_control.ShowAll = true;
			_control.ShowDebug = false;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowOther.Should().BeTrue();
			_control.ShowTrace.Should().BeTrue();
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		public void TestShowAll4()
		{
			_control.ShowAll = false;
			_control.ShowDebug = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowOther.Should().BeFalse();
			_control.ShowTrace.Should().BeFalse();
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		public void TestShowAll5()
		{
			_control.ShowAll = false;
			_control.ShowAll.Should().BeFalse();

			_control.ShowOther = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowTrace = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowDebug = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowInfo = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowWarning = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowError = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowFatal = true;
			_control.ShowAll.Should().BeTrue();
		}

		[Test]
		[Description(
			"Verifies that when the data source's selected log lines change, then the control synchronizes itself properly")]
		public void TestChangeSelectedLogLines()
		{
			_dataSource.SelectedLogLines = new HashSet<LogLineIndex>
			{
				new LogLineIndex(42),
				new LogLineIndex(108)
			};
			_control.SelectedIndices.Should().BeEquivalentTo(new[]
			{
				new LogLineIndex(42),
				new LogLineIndex(108)
			}, "because the control is expected to listen to changes of the data source");
		}

		[Test]
		[Description(
			"Verifies that when the data source's visible log line changes, then the control synchronizes itself properly")]
		public void TestChangeVisibleLogLine()
		{
			_dataSource.VisibleLogLine = new LogLineIndex(9001);
			_control.CurrentLogLine.Should()
				.Be(new LogLineIndex(9001), "because the control is expected to listen to changes of the data source");
		}

		[Test]
		[Description(
			"Verifies that changes to the MergedDataSourceDisplayMode property are forwarded to the data source view model")]
		public void TestChangeMergedDataSourceDisplayMode1()
		{
			var dataSource = new Mock<IMergedDataSourceViewModel>();
			dataSource.SetupProperty(x => x.DisplayMode);

			_control.DataSource = dataSource.Object;

			_control.MergedDataSourceDisplayMode = DataSourceDisplayMode.CharacterCode;
			dataSource.Object.DisplayMode.Should().Be(DataSourceDisplayMode.CharacterCode);

			_control.MergedDataSourceDisplayMode = DataSourceDisplayMode.Filename;
			dataSource.Object.DisplayMode.Should().Be(DataSourceDisplayMode.Filename);
		}

		[Test]
		[Description(
			"Verifies that changes to the MergedDataSourceDisplayMode property are ignored if the view model isn't a merged one")
		]
		public void TestChangeMergedDataSourceDisplayMode2()
		{
			var dataSource = new Mock<IDataSourceViewModel>();

			_control.DataSource = dataSource.Object;

			new Action(() => _control.MergedDataSourceDisplayMode = DataSourceDisplayMode.CharacterCode).Should().NotThrow();
			new Action(() => _control.MergedDataSourceDisplayMode = DataSourceDisplayMode.Filename).Should().NotThrow();
		}

		[Test]
		[Description("Verifies that the display mode of the new data source is used")]
		public void TestChangeDataSource(
			[Values(DataSourceDisplayMode.Filename, DataSourceDisplayMode.CharacterCode)] DataSourceDisplayMode displayMode)
		{
			var dataSource = new Mock<IMergedDataSourceViewModel>();
			dataSource.SetupProperty(x => x.DisplayMode);
			dataSource.Object.DisplayMode = displayMode;

			_control.DataSource = dataSource.Object;
			_control.MergedDataSourceDisplayMode.Should()
				.Be(displayMode,
					"because the view model determines the initial display mode and thus the control should just use that");
			dataSource.Object.DisplayMode.Should()
				.Be(displayMode, "because the display mode of the view model shouldn't have been changed in the process");
		}

		[Test]
		[Description("Verifies that the settings are simply forwarded to the LogEntryListView")]
		public void TestChangeSettings()
		{
			var settings = new LogViewerSettings();

			_control.Settings = settings;
			_control.PART_ListView.Settings.Should().BeSameAs(settings);
		}
	}
}