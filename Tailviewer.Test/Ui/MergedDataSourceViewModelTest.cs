using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Scheduling;
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
			_scheduler = new TaskScheduler();
			_dataSources = new DataSources(_scheduler, _settings);
			_model = new MergedDataSourceViewModel(_dataSources.AddGroup());
		}

		[Test]
		public void TearDown()
		{
			_scheduler.Dispose();
		}

		private MergedDataSourceViewModel _model;
		private DataSources _dataSources;
		private Tailviewer.Settings.DataSources _settings;
		private TaskScheduler _scheduler;

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