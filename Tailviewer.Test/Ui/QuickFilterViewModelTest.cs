using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class QuickFilterViewModelTest
	{
		private QuickFilter _quickFilter;
		private DataSource _dataSource;
		private QuickFilterViewModel _model;
		private Tailviewer.Settings.DataSource _dataSourceSettings;

		[SetUp]
		public void SetUp()
		{
			_quickFilter = new QuickFilter(new Tailviewer.Settings.QuickFilter());
			_dataSource = new DataSource(_dataSourceSettings = new Tailviewer.Settings.DataSource("nothing"));
			_model = new QuickFilterViewModel(_quickFilter, x => { }, _dataSource);
		}

		[Test]
		public void TestCtor()
		{
			_model.IsActive.Should().BeFalse("Because the quickfilter's guid is not the data source's list");
		}

		[Test]
		public void TestDataSource()
		{
			_model.DataSource.Should().NotBeNull();
			_model.CanBeActivated.Should().BeTrue();
			_model.DataSource = null;
			_model.CanBeActivated.Should().BeFalse();
			_model.IsActive.Should().BeFalse();
			new Action(() => _model.IsActive = true).ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void TestIsActive()
		{
			_model.IsActive = true;
			_dataSourceSettings.ActivatedQuickFilters.Should().Equal(new[] { _quickFilter.Id });

			_model.IsActive = false;
			_dataSourceSettings.ActivatedQuickFilters.Should().BeEmpty();
		}
	}
}