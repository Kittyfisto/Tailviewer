using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class QuickFiltersViewModelTest
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings("addwa");
			_quickFilters = new QuickFilters(_settings.QuickFilters);
		}

		private QuickFilters _quickFilters;
		private ApplicationSettings _settings;
		private ManualTaskScheduler _scheduler;

		[Test]
		public void TestAdd()
		{
			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			var dataSource = new SingleDataSource(_scheduler, new DataSource("sw") {Id = Guid.NewGuid()});
			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource);
			QuickFilterViewModel filter = model.AddQuickFilter();
			filter.CurrentDataSource.Should().BeSameAs(dataSource);
		}

		[Test]
		public void TestChangeCurrentDataSource()
		{
			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			var dataSource = new SingleDataSource(_scheduler, new DataSource("sw") { Id = Guid.NewGuid() });
			QuickFilterViewModel filter = model.AddQuickFilter();
			filter.CurrentDataSource.Should().BeNull();

			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource);
			filter.CurrentDataSource.Should().BeSameAs(dataSource);

			model.CurrentDataSource = null;
			filter.CurrentDataSource.Should().BeNull();
		}

		[Test]
		public void TestChangeFilterType1()
		{
			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			model.CurrentDataSource =
				new SingleDataSourceViewModel(new SingleDataSource(_scheduler, new DataSource("adw") { Id = Guid.NewGuid() }));

			int numFilterChanges = 0;
			QuickFilterViewModel filter = model.AddQuickFilter();
			filter.MatchType.Should().Be(QuickFilterMatchType.StringFilter);
			filter.Value = "Foobar";
			filter.IsActive = true;

			model.OnFiltersChanged += () => ++numFilterChanges;
			filter.MatchType = QuickFilterMatchType.WildcardFilter;
			numFilterChanges.Should().Be(1);
		}

		[Test]
		public void TestCreateFilterChain()
		{
			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			model.CreateFilterChain().Should().BeNull();
		}

		[Test]
		public void TestCtor1()
		{
			_quickFilters.Add().Value = "foo";
			_quickFilters.Add().Value = "bar";

			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			List<QuickFilterViewModel> filters = model.Observable.ToList();
			filters.Count.Should().Be(2);
			filters[0].Value.Should().Be("foo");
			filters[1].Value.Should().Be("bar");
		}

		[Test]
		public void TestCtor2()
		{
			QuickFilter filter1 = _quickFilters.Add();
			var dataSource = new SingleDataSource(_scheduler, new DataSource("daw") { Id = Guid.NewGuid() });
			dataSource.ActivateQuickFilter(filter1.Id);

			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			int changed = 0;
			model.OnFiltersChanged += () => ++changed;
			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource);
			changed.Should().Be(1, "Because changing the current data source MUST apply ");
		}

		[Test]
		[Description("Verifies that removing an active quick-filter causes the OnFiltersChanged event to be fired")]
		public void TestRemove1()
		{
			QuickFilter filter1 = _quickFilters.Add();
			var dataSource = new SingleDataSource(_scheduler, new DataSource("daw") { Id = Guid.NewGuid() });
			dataSource.ActivateQuickFilter(filter1.Id);

			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource);
			var filter1Model = model.Observable.First();

			int changed = 0;
			model.OnFiltersChanged += () => ++changed;

			filter1Model.RemoveCommand.Execute(null);
			model.Observable.Should().BeEmpty("because we've just removed the only quick filter");
			changed.Should().Be(1, "because removing an active quick-filter should always fire the OnFiltersChanged event");
		}

		[Test]
		[Description("Verifies that removing an inactive quick-filter does NOT cause the OnFiltersChanged event to be fired")]
		public void TestRemove2()
		{
			QuickFilter filter1 = _quickFilters.Add();
			var dataSource = new SingleDataSource(_scheduler, new DataSource("daw") { Id = Guid.NewGuid() });
			dataSource.ActivateQuickFilter(filter1.Id);

			var model = new QuickFiltersViewModel(_settings, _quickFilters);
			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource);
			var filter1Model = model.Observable.First();
			filter1Model.IsActive = false;

			int changed = 0;
			model.OnFiltersChanged += () => ++changed;

			filter1Model.RemoveCommand.Execute(null);
			model.Observable.Should().BeEmpty("because we've just removed the only quick filter");
			changed.Should().Be(0, "because removing an inactive quick-filter should never fire the OnFiltersChanged event");
		}
	}
}