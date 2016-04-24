using System;
using FluentAssertions;
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
		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_control = new LogViewerControl
				{
					DataSource = new SingleDataSourceViewModel(new SingleDataSource(new DataSource("Foobar") {Id = Guid.NewGuid()}))
				};
			_dispatcher = new ManualDispatcher();
		}

		private LogViewerControl _control;
		private ManualDispatcher _dispatcher;


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
			var source = new SingleDataSourceViewModel(new SingleDataSource(new DataSource("Foobar") {Id = Guid.NewGuid()}));
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

		[Test]
		[STAThread]
		[Ignore("Doesn't work yet")]
		[Description("Verifies that upon setting the data source, the FollowTail property is forwarded to the LogEntryListView")]
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

		[Test][Ignore("Not complete yet")]
		[STAThread]
		[Description("Verifies that changing the LogView does NOT change the currently visible line of the old view")]
		public void TestChangeLogView1()
		{
			// TODO: This test requires that the template be fully loaded (or the control changed to a user control)

			var oldLog = new Mock<IDataSourceViewModel>();
			var oldDataSource = new Mock<IDataSource>();
			oldDataSource.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			oldDataSource.Setup(x => x.LogFile).Returns(new Mock<ILogFile>().Object);
			oldLog.Setup(x => x.DataSource).Returns(oldDataSource.Object);
			oldLog.SetupProperty(x => x.VisibleLogLine);
			oldLog.Object.VisibleLogLine = 42;
			var oldLogView = new LogViewerViewModel(oldLog.Object, _dispatcher);
			_control.LogView = oldLogView;


			var newLog = new Mock<IDataSourceViewModel>();
			var newDataSource = new Mock<IDataSource>();
			newDataSource.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			newDataSource.Setup(x => x.LogFile).Returns(new Mock<ILogFile>().Object);
			newLog.Setup(x => x.DataSource).Returns(newDataSource.Object);
			newLog.Setup(x => x.VisibleLogLine).Returns(1);
			var newLogView = new LogViewerViewModel(newLog.Object, _dispatcher);
			_control.LogView = newLogView;

			oldLog.Object.VisibleLogLine.Should().Be(42, "Because the control shouldn't have changed the VisibleLogLine of the old logview");
		}
	}
}