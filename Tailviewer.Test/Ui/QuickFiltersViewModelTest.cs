using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;
using DataSource = Tailviewer.BusinessLogic.DataSource;
using QuickFilters = Tailviewer.BusinessLogic.QuickFilters;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class QuickFiltersViewModelTest
	{
		private QuickFilters _quickFilters;
		private ApplicationSettings _settings;

		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings();
			_quickFilters = new QuickFilters(_settings.QuickFilters);
		}

		[Test]
		public void TestCtor()
		{
			_quickFilters.Add().Value = "foo";
			_quickFilters.Add().Value = "bar";

			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			var filters = model.Observable.ToList();
			filters.Count.Should().Be(2);
			filters[0].Value.Should().Be("foo");
			filters[1].Value.Should().Be("bar");
		}

		[Test]
		public void TestAdd()
		{
			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			var dataSource = new DataSource(new Tailviewer.Settings.DataSource("sw"));
			model.CurrentDataSource = new DataSourceViewModel(dataSource);
			var filter = model.AddQuickFilter();
			filter.CurrentDataSource.Should().BeSameAs(dataSource);
		}

		[Test]
		public void TestChangeCurrentDataSource()
		{
			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			var dataSource = new DataSource(new Tailviewer.Settings.DataSource("sw"));
			var filter = model.AddQuickFilter();
			filter.CurrentDataSource.Should().BeNull();

			model.CurrentDataSource = new DataSourceViewModel(dataSource);
			filter.CurrentDataSource.Should().BeSameAs(dataSource);

			model.CurrentDataSource = null;
			filter.CurrentDataSource.Should().BeNull();
		}
	}
}