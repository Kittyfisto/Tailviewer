using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.Properties;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class AbstractDataSourceViewModelTest
	{
		[SetUp]
		public void SetUp()
		{
			_settings = new DataSource();

			_logFile = new Mock<ILogSource>();

			_dataSource = new Mock<IDataSource>();
			_dataSource.Setup(x => x.UnfilteredLogSource).Returns(_logFile.Object);
			_dataSource.Setup(x => x.Settings).Returns(_settings);
			_dataSource.SetupProperty(x => x.LastViewed);
			_dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);
			_dataSource.SetupProperty(x => x.VisibleLogLine);

			var actionCenter = new Mock<IActionCenter>();
			var applicationSettings = new Mock<IApplicationSettings>();

			_viewModel = new DataSourceViewModel(_dataSource.Object, actionCenter.Object, applicationSettings.Object);
		}

		private Mock<IDataSource> _dataSource;
		private DataSourceViewModel _viewModel;
		private DataSource _settings;
		private Mock<ILogSource> _logFile;

		private sealed class DataSourceViewModel
			: AbstractDataSourceViewModel
		{
			public DataSourceViewModel(IDataSource dataSource, IActionCenter actionCenter, IApplicationSettings applicationSettings)
				: base(dataSource, actionCenter, applicationSettings)
			{
				Update();
			}

			public override string DisplayName
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public override bool CanBeRenamed
			{
				get { throw new NotImplementedException(); }
			}

			public override string DataSourceOrigin
			{
				get { throw new NotImplementedException(); }
			}
		}

		[Test]
		public void TestCtor()
		{
			_viewModel.SearchResultCount.Should().Be(0);
			_viewModel.CurrentSearchResultIndex.Should().Be(-1);
		}

		[Test]
		[Description(
			"Verifies that the number of new log lines is NOT increased when the last modified timestamp is less than the last viewed one"
			)]
		public void TestAddLine()
		{
			_viewModel.IsVisible = true;
			_viewModel.IsVisible = false;

			var changes = new List<string>();
			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);
			_viewModel.NewLogLineCount.Should().Be(0);

			DateTime now = DateTime.Now;
			_dataSource.Setup(x => x.TotalCount).Returns(1);
			_dataSource.Setup(x => x.LastModified).Returns(now - TimeSpan.FromMinutes(1));

			_viewModel.Update();
			_viewModel.NewLogLineCount.Should()
			          .Be(0,
			              "Because even though the number of log lines has changed - it is not reported as new because the last modified timestamp is still in the past");
			changes.Should().Equal("TotalCount", "LastWrittenAge");
		}

		[Test]
		[Description("Verifies that changes made to the total count of a log file are not counted as new" +
		             "unless the last modified timestamp is GREATER than the timestamp the file was last seen")]
		public void TestAddLine2()
		{
			_viewModel.IsVisible = true;
			_viewModel.IsVisible = false;

			var changes = new List<string>();
			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);
			_viewModel.NewLogLineCount.Should().Be(0);

			_dataSource.Setup(x => x.TotalCount).Returns(1);
			_dataSource.Setup(x => x.LastModified).Returns(DateTime.Now);

			_viewModel.Update();
			_viewModel.NewLogLineCount.Should().Be(1);
			changes.Should().Equal("TotalCount", "LastWrittenAge", "NewLogLineCount");
		}

		[Test]
		[Description("Verifies that the number of new log lines is NOT increased when IsVisible is set to true")]
		public void TestAddLine3()
		{
			_viewModel.IsVisible = true;

			var changes = new List<string>();
			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);
			_viewModel.NewLogLineCount.Should().Be(0);

			DateTime now = DateTime.Now;
			_dataSource.Setup(x => x.TotalCount).Returns(1);
			_dataSource.Setup(x => x.LastModified).Returns(now);

			_viewModel.Update();
			_viewModel.NewLogLineCount.Should()
			          .Be(0,
			              "Because even though the number of log lines has changed - it is not reported as new because the last modified timestamp is still in the past");
			changes.Should().Equal("TotalCount", "LastWrittenAge");
		}

		[Test]
		[Description("Verifies that setting the IsVisible to true causes the LastSeen timestamp to be set to now")]
		public void TestIsVisible()
		{
			_viewModel.IsVisible.Should().BeFalse();

			DateTime before = DateTime.Now;
			_viewModel.IsVisible = true;
			DateTime after = DateTime.Now;
			_viewModel.LastViewed.Should().BeOnOrAfter(before);
			_viewModel.LastViewed.Should().BeOnOrBefore(after);
		}

		[Test]
		[Description("Verifies that the view model assumes that the log was cleared when the number of lines decreases")]
		public void TestRemoveLine()
		{
			var changes = new List<string>();
			_dataSource.Setup(x => x.TotalCount).Returns(10);
			_dataSource.Setup(x => x.LastModified).Returns(DateTime.MinValue);
			_viewModel.Update();

			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);
			_dataSource.Setup(x => x.TotalCount).Returns(4);
			_viewModel.Update();

			_viewModel.NewLogLineCount.Should().Be(4);
			changes.Count.Should().BeInRange(2, 3);
			changes.Should().Contain("TotalCount");
			changes.Should().Contain("NewLogLineCount");
		}

		[Test]
		public void TestExists1()
		{
			_logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			_viewModel.Update();
			_viewModel.Exists.Should().BeTrue();
		}

		[Test]
		public void TestExists2([Values(ErrorFlags.SourceCannotBeAccessed, ErrorFlags.SourceDoesNotExist)] ErrorFlags emptyReason)
		{
			_logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(emptyReason);
			_viewModel.Update();
			_viewModel.Exists.Should().BeFalse();
		}

		[Test]
		public void TestExists3()
		{
			_viewModel.Exists.Should().BeTrue();

			var changes = new List<string>();
			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);

			_logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.SourceDoesNotExist);
			_viewModel.Update();
			_viewModel.Exists.Should().BeFalse();

			_logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			_viewModel.Update();
			_viewModel.Exists.Should().BeTrue();
			changes.Should().Equal("Exists", "Exists");
		}

		[Test]
		public void TestVisibleLogLine()
		{
			var changes = new List<string>();
			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);

			_viewModel.VisibleLogLine = new LogLineIndex(108);
			changes.Should().Equal("VisibleLogLine");

			changes.Clear();
			_viewModel.VisibleLogLine = new LogLineIndex(108);
			changes.Should().BeEmpty("Because the property hasn't changed");
		}
	}
}