using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.Settings;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.QuickFilter;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class QuickFilterViewModelTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
		}

		[SetUp]
		public void SetUp()
		{
			_quickFilter = new QuickFilter(new Core.Settings.QuickFilter());
			_dataSource = new SingleDataSource(_logFileFactory, _scheduler, _dataSourceSettings = new DataSource("nothing") {Id = DataSourceId.CreateNew()});
			_model = new QuickFilterViewModel(_quickFilter, x => { })
				{
					CurrentDataSource = _dataSource
				};
			_changes = new List<string>();
			_model.PropertyChanged += (sender, args) => _changes.Add(args.PropertyName);
		}

		private QuickFilter _quickFilter;
		private SingleDataSource _dataSource;
		private QuickFilterViewModel _model;
		private DataSource _dataSourceSettings;
		private List<string> _changes;
		private ManualTaskScheduler _scheduler;
		private ILogFileFactory _logFileFactory;

		[Test]
		public void TestChangeType()
		{
			_model.MatchType = QuickFilterMatchType.RegexpFilter;
			_changes.Should().Equal(new[] {"MatchType"});
			_model.MatchType = QuickFilterMatchType.RegexpFilter;
			_changes.Should().Equal(new[] {"MatchType"});
		}

		[Test]
		public void TestCtor()
		{
			_model.CurrentDataSource.Should().NotBeNull();
			_model.IsActive.Should().BeFalse("Because the quickfilter's guid is not the data source's list");
			_model.CanBeActivated.Should().BeTrue();
			_model.RemoveCommand.Should().NotBeNull();
			_model.RemoveCommand.CanExecute(null).Should().BeTrue();
		}

		[Test]
		public void TestDataSource1()
		{
			_model.CurrentDataSource.Should().NotBeNull();
			_model.CanBeActivated.Should().BeTrue();
			_model.CurrentDataSource = null;
			_model.CanBeActivated.Should().BeFalse();
			_model.IsActive.Should().BeFalse();
			new Action(() => _model.IsActive = true).ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void TestDataSource2()
		{
			_model.CurrentDataSource.Should().NotBeNull();
			_model.CanBeActivated.Should().BeTrue();
			_model.CurrentDataSource = null;
			_changes.Should().Equal(new[] {"CanBeActivated"});
			_model.CanBeActivated.Should().BeFalse();
			_model.IsActive.Should().BeFalse();

			_model.CurrentDataSource = _dataSource;
			_changes.Should().Equal(new[] {"CanBeActivated", "CanBeActivated"});

			_model.CurrentDataSource = _dataSource;
			_changes.Should().Equal(new[] {"CanBeActivated", "CanBeActivated"});
		}

		[Test]
		public void TestIsInverted()
		{
			_model.IsInverted = true;
			_quickFilter.IsInverted.Should().BeTrue();

			_model.IsInverted = false;
			_quickFilter.IsInverted.Should().BeFalse();

			_model.IsInverted = true;
			_quickFilter.IsInverted.Should().BeTrue();
		}

		[Test]
		public void TestIsActive()
		{
			_model.IsActive = true;
			_dataSourceSettings.ActivatedQuickFilters.Should().Equal(new[] {_quickFilter.Id});

			_model.IsActive = false;
			_dataSourceSettings.ActivatedQuickFilters.Should().BeEmpty();
		}

		[Test]
		[Enhancement("https://github.com/Kittyfisto/Tailviewer/issues/82")]
		public void TestChangeFilterText1()
		{
			_model.IsActive.Should().BeFalse();
			_model.Value = "id = 42";
			_model.IsActive.Should().BeTrue("because a filter should activate itself when the text is being modified");
		}

		[Test]
		[Enhancement("https://github.com/Kittyfisto/Tailviewer/issues/82")]
		public void TestChangeFilterText2()
		{
			_model.IsActive.Should().BeFalse();
			_model.CurrentDataSource = null;
			_model.Value = "id = 42";
			_model.IsActive.Should().BeFalse("because a filter that doesn't control a data source cannot be activated");
		}
	}
}