using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class MergedDataSourceViewModelTest
	{
		[SetUp]
		public void SetUp()
		{
			_settings = new Tailviewer.Settings.DataSources();
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
			_dataSources = new DataSources(_logFileFactory, _scheduler, _settings);
			
		}

		private DataSources _dataSources;
		private Tailviewer.Settings.DataSources _settings;
		private ManualTaskScheduler _scheduler;
		private ILogFileFactory _logFileFactory;

		[Test]
		public void TestConstruction1()
		{
			var model = new MergedDataSourceViewModel(_dataSources.AddGroup());
			model.IsSelected.Should().BeFalse();
			model.IsExpanded.Should().BeTrue();
		}

		[Test]
		public void TestConstruction2([Values(DataSourceDisplayMode.Filename, DataSourceDisplayMode.CharacterCode)] DataSourceDisplayMode displayMode)
		{
			var dataSource = _dataSources.AddGroup();
			dataSource.DisplayMode = displayMode;

			var model = new MergedDataSourceViewModel(dataSource);
			model.DisplayMode.Should().Be(displayMode);
		}

		[Test]
		public void TestExpand()
		{
			var dataSource = _dataSources.AddGroup();
			var model = new MergedDataSourceViewModel(dataSource);
			model.IsExpanded = false;
			model.IsExpanded.Should().BeFalse();
			dataSource.IsExpanded.Should().BeFalse();

			model.IsExpanded = true;
			model.IsExpanded.Should().BeTrue();
			dataSource.IsExpanded.Should().BeTrue();
		}

		[Test]
		public void TestAdd1()
		{
			var model = new MergedDataSourceViewModel(_dataSources.AddGroup());
			SingleDataSource source = _dataSources.AddDataSource("foo");
			var sourceViewModel = new SingleDataSourceViewModel(source);
			model.AddChild(sourceViewModel);
			model.Observable.Should().Equal(sourceViewModel);
			sourceViewModel.Parent.Should().BeSameAs(model);
		}

		[Test]
		public void TestChangeDisplayMode()
		{
			var dataSource = _dataSources.AddGroup();
			var model = new MergedDataSourceViewModel(dataSource);

			model.DisplayMode = DataSourceDisplayMode.CharacterCode;
			dataSource.DisplayMode.Should().Be(DataSourceDisplayMode.CharacterCode);

			model.DisplayMode = DataSourceDisplayMode.Filename;
			dataSource.DisplayMode.Should().Be(DataSourceDisplayMode.Filename);
		}
	}
}