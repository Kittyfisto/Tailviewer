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
			_settings = new ApplicationSettings("addwa");
			_quickFilters = new QuickFilters(_settings.QuickFilters);
		}

		[Test]
		public void TestCtor1()
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
		public void TestCtor2()
		{
			var filter1 = _quickFilters.Add();
			var dataSource = new DataSource(new Tailviewer.Settings.DataSource("daw"));
			dataSource.ActivateQuickFilter(filter1.Id);

			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			int changed = 0;
			model.OnFiltersChanged += () => ++changed;
			model.CurrentDataSource = new DataSourceViewModel(dataSource);
			changed.Should().Be(1, "Because changing the current data source MUST apply ");
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
		public void TestCreateFilterChain()
		{
			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			model.CreateFilterChain().Should().BeNull();
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