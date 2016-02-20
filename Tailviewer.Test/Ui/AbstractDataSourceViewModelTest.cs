using System;
using System.Collections.Generic;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class AbstractDataSourceViewModelTest
	{
		private Mock<IDataSource> _dataSource;
		private DataSourceViewModel _viewModel;
		private DataSource _settings;
		private Mock<ILogFile> _logFile;

		private sealed class DataSourceViewModel
			: AbstractDataSourceViewModel
		{
			public DataSourceViewModel(IDataSource dataSource) : base(dataSource)
			{
			}

			public override ICommand OpenInExplorerCommand
			{
				get { throw new NotImplementedException(); }
			}

			public override string DisplayName
			{
				get { throw new NotImplementedException(); }
			}
		}

		[SetUp]
		public void SetUp()
		{
			_settings = new DataSource();
			_dataSource = new Mock<IDataSource>();
			_logFile = new Mock<ILogFile>();
			_dataSource.Setup(x => x.LogFile).Returns(_logFile.Object);
			_dataSource.Setup(x => x.Settings).Returns(_settings);
			_viewModel = new DataSourceViewModel(_dataSource.Object);
		}

		[Test]
		[Description("Verifies that the number of new log lines is increased when a line is added")]
		public void TestAddLine()
		{
			var changes = new List<string>();
			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);
			_viewModel.NewLogLineCount.Should().Be(0);
			_dataSource.Setup(x => x.TotalCount).Returns(1);
			_viewModel.Update();
			_viewModel.NewLogLineCount.Should().Be(1);
			changes.Should().Equal(new[] { "TotalCount", "LastWrittenAge", "NewLogLineCount" });
		}

		[Test]
		[Description("Verifies that the view model assumes that the log was cleared when the number of lines decreases")]
		public void TestRemoveLine()
		{
			var changes = new List<string>();
			_dataSource.Setup(x => x.TotalCount).Returns(10);
			_viewModel.Update();

			_viewModel.PropertyChanged += (unused, args) => changes.Add(args.PropertyName);
			_dataSource.Setup(x => x.TotalCount).Returns(4);
			_viewModel.Update();

			_viewModel.NewLogLineCount.Should().Be(4);
			changes.Should().Equal(new[] { "TotalCount", "LastWrittenAge", "NewLogLineCount" });
		}
	}
}