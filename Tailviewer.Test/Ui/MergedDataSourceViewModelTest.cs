using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.ViewModels;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class MergedDataSourceViewModelTest
	{
		private MergedDataSourceViewModel _model;
		private DataSources _dataSources;
		private Tailviewer.Settings.DataSources _settings;

		[SetUp]
		public void SetUp()
		{
			_settings = new Tailviewer.Settings.DataSources();
			_dataSources = new DataSources(_settings);
			_model = new MergedDataSourceViewModel(_dataSources.AddGroup());
		}

		[Test]
		public void TestAdd1()
		{
			var source = _dataSources.AddDataSource("foo");
			var sourceViewModel = new SingleDataSourceViewModel(source);
			_model.AddChild(sourceViewModel);
			_model.Observable.Should().Equal(sourceViewModel);
			sourceViewModel.Parent.Should().BeSameAs(_model);
		}
	}
}