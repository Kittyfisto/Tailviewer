using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Settings;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.ViewModels;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class QuickFiltersViewModelTest
	{
		private ILogFileFactory _logFileFactory;
		private QuickFilters _quickFilters;
		private ManualTaskScheduler _scheduler;
		private ApplicationSettings _settings;
		private Mock<IActionCenter> _actionCenter;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
			_actionCenter = new Mock<IActionCenter>();
		}

		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings("addwa");
			_quickFilters = new QuickFilters(_settings.QuickFilters);
		}

		[Test]
		public void TestAdd()
		{
			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters);
			var dataSource = new SingleDataSource(_logFileFactory, _scheduler,
				new DataSource("sw") {Id = DataSourceId.CreateNew()});
			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource, _actionCenter.Object);
			var filter = model.AddQuickFilter();
			filter.CurrentDataSource.Should().BeSameAs(dataSource);
		}

		[Test]
		public void TestChangeCurrentDataSource()
		{
			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters);
			var dataSource = new SingleDataSource(_logFileFactory, _scheduler,
				new DataSource("sw") {Id = DataSourceId.CreateNew()});
			var filter = model.AddQuickFilter();
			filter.CurrentDataSource.Should().BeNull();

			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource, _actionCenter.Object);
			filter.CurrentDataSource.Should().BeSameAs(dataSource);

			model.CurrentDataSource = null;
			filter.CurrentDataSource.Should().BeNull();
		}

		[Test]
		public void TestChangeFilterType1()
		{
			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters)
			{
				CurrentDataSource = new SingleDataSourceViewModel(new SingleDataSource(_logFileFactory, _scheduler,
					new DataSource("adw") {Id = DataSourceId.CreateNew()}), _actionCenter.Object)
			};

			var numFilterChanges = 0;
			var filter = model.AddQuickFilter();
			filter.MatchType.Should().Be(FilterMatchType.SubstringFilter);
			filter.Value = "Foobar";
			filter.IsActive = true;

			model.OnFiltersChanged += () => ++numFilterChanges;
			filter.MatchType = FilterMatchType.WildcardFilter;
			numFilterChanges.Should().Be(1);
		}

		[Test]
		public void TestCreateFilterChain()
		{
			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters);
			model.CreateFilterChain().Should().BeNull();
		}

		[Test]
		public void TestCtor1()
		{
			_quickFilters.Add().Value = "foo";
			_quickFilters.Add().Value = "bar";

			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters);
			var filters = model.QuickFilters.ToList();
			filters.Count.Should().Be(2);
			filters[0].Value.Should().Be("foo");
			filters[1].Value.Should().Be("bar");
		}

		[Test]
		public void TestCtor2()
		{
			var filter1 = _quickFilters.Add();
			var dataSource = new SingleDataSource(_logFileFactory, _scheduler,
				new DataSource("daw") {Id = DataSourceId.CreateNew()});
			dataSource.ActivateQuickFilter(filter1.Id);

			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters);
			var changed = 0;
			model.OnFiltersChanged += () => ++changed;
			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource, _actionCenter.Object);
			changed.Should().Be(1, "Because changing the current data source MUST apply ");
		}

		[Test]
		[Description("Verifies that removing an active quick-filter causes the OnFiltersChanged event to be fired")]
		public void TestRemove1()
		{
			var filter1 = _quickFilters.Add();
			var dataSource = new SingleDataSource(_logFileFactory, _scheduler,
				new DataSource("daw") {Id = DataSourceId.CreateNew()});
			dataSource.ActivateQuickFilter(filter1.Id);

			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters)
			{
				CurrentDataSource = new SingleDataSourceViewModel(dataSource, _actionCenter.Object)
			};
			var filter1Model = model.QuickFilters.First();

			var changed = 0;
			model.OnFiltersChanged += () => ++changed;

			filter1Model.RemoveCommand.Execute(null);
			model.QuickFilters.Should().BeEmpty("because we've just removed the only quick filter");
			changed.Should().Be(1, "because removing an active quick-filter should always fire the OnFiltersChanged event");
		}

		[Test]
		[Description("Verifies that removing an inactive quick-filter does NOT cause the OnFiltersChanged event to be fired")]
		public void TestRemove2()
		{
			var filter1 = _quickFilters.Add();
			var dataSource = new SingleDataSource(_logFileFactory, _scheduler,
				new DataSource("daw") {Id = DataSourceId.CreateNew()});
			dataSource.ActivateQuickFilter(filter1.Id);

			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters)
			{
				CurrentDataSource = new SingleDataSourceViewModel(dataSource, _actionCenter.Object)
			};
			var filter1Model = model.QuickFilters.First();
			filter1Model.IsActive = false;

			var changed = 0;
			model.OnFiltersChanged += () => ++changed;

			filter1Model.RemoveCommand.Execute(null);
			model.QuickFilters.Should().BeEmpty("because we've just removed the only quick filter");
			changed.Should().Be(0, "because removing an inactive quick-filter should never fire the OnFiltersChanged event");
		}

		[Test]
		public void TestActivate1()
		{
			_quickFilters.Add();
			var model = new QuickFiltersSidePanelViewModel(_settings, _quickFilters);
			model.QuickInfo.Should().BeNull();

			var dataSource = new SingleDataSource(_logFileFactory, _scheduler,
				new DataSource("daw") {Id = DataSourceId.CreateNew()});
			model.CurrentDataSource = new SingleDataSourceViewModel(dataSource, _actionCenter.Object);
			model.QuickFilters.ElementAt(0).IsActive = true;
			model.QuickInfo.Should().Be("1 active");

			model.AddQuickFilter();
			model.QuickFilters.ElementAt(1).IsActive = true;
			model.QuickInfo.Should().Be("2 active");

			model.QuickFilters.ElementAt(0).IsActive = false;
			model.QuickFilters.ElementAt(1).IsActive = false;
			model.QuickInfo.Should().BeNull();
		}
	}
}