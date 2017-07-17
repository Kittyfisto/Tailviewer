using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.ViewModels;

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
			_model = new MergedDataSourceViewModel(_settingsDataSource = _dataSources.AddGroup());
		}

		private MergedDataSourceViewModel _model;
		private DataSources _dataSources;
		private Tailviewer.Settings.DataSources _settings;
		private ManualTaskScheduler _scheduler;
		private MergedDataSource _settingsDataSource;
		private ILogFileFactory _logFileFactory;

		[Test]
		public void TestConstruction()
		{
			_model.IsSelected.Should().BeFalse();
			_model.IsExpanded.Should().BeTrue();
		}

		[Test]
		public void TestExpand()
		{
			_model.IsExpanded = false;
			_model.IsExpanded.Should().BeFalse();
			_settingsDataSource.IsExpanded.Should().BeFalse();

			_model.IsExpanded = true;
			_model.IsExpanded.Should().BeTrue();
			_settingsDataSource.IsExpanded.Should().BeTrue();
		}

		[Test]
		public void TestAdd1()
		{
			SingleDataSource source = _dataSources.AddDataSource("foo");
			var sourceViewModel = new SingleDataSourceViewModel(source);
			_model.AddChild(sourceViewModel);
			_model.Observable.Should().Equal(sourceViewModel);
			sourceViewModel.Parent.Should().BeSameAs(_model);
		}
	}
}