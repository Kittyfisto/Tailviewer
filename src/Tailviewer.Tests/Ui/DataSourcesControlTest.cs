using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Tests.Ui
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class DataSourcesControlTest
	{
		private DataSourcesControl _control;

		[SetUp]
		public void SetUp()
		{
			_control = new DataSourcesControl();
		}

		[Pure]
		private IDataSourceViewModel CreateViewModel(string displayName)
		{
			var viewModel = new Mock<IDataSourceViewModel>();
			viewModel.Setup(x => x.DisplayName).Returns(displayName);
			return viewModel.Object;
		}

		[Test]
		public void TestAddRemove()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>();
			_control.ItemsSource = sources;
			_control.NoDataSourcesReason.Should().Be("No data source opened");
			_control.NoDataSourcesActions.Should().Be("Try opening a file or folder, create a new data source from the file menu or simply drag and drop a file into this window.");

			sources.Add(new Mock<IDataSourceViewModel>().Object);
			_control.NoDataSourcesReason.Should().BeNull("because we just added a data source");
			_control.NoDataSourcesActions.Should().BeNull("because we just added a data source");

			sources.Clear();
			_control.NoDataSourcesReason.Should().Be("No data source opened");
			_control.NoDataSourcesActions.Should().Be("Try opening a file or folder, create a new data source from the file menu or simply drag and drop a file into this window.");
		}

		[Test]
		public void TestSetItemsSource()
		{
			var sources = new ObservableCollection<IDataSourceViewModel> {new Mock<IDataSourceViewModel>().Object};
			_control.ItemsSource = sources;
			_control.NoDataSourcesReason.Should().BeNull("because we just added a data source");
			_control.NoDataSourcesActions.Should().BeNull("because we just added a data source");

			_control.ItemsSource = new List<IDataSourceViewModel>();
			_control.NoDataSourcesReason.Should().Be("No data source opened");
			_control.NoDataSourcesActions.Should().Be("Try opening a file or folder, create a new data source from the file menu or simply drag and drop a file into this window.");
		}

		[Test]
		public void TestFilter1()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>();
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().BeEmpty();
		}

		[Test]
		public void TestFilter2()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log")
			};
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		public void TestFilter3()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>();
			_control.ItemsSource = sources;

			sources.Add(
			            CreateViewModel("test.log"));
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.Add(
			            CreateViewModel("test2.log"));
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.Add(
			            CreateViewModel("test3.log"));
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		public void TestFilter4()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>();
			_control.ItemsSource = sources;

			sources.Add(
			            CreateViewModel("test.log"));
			sources.Add(
			            CreateViewModel("test2.log"));
			sources.Add(
			            CreateViewModel("test3.log"));

			sources.RemoveAt(1);
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		public void TestFilter5()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log"),
				CreateViewModel("test2.log"),
				CreateViewModel("test3.log")
			};
			_control.StringFilter = "2";
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().Equal(sources[1]);
		}

		[Test]
		public void TestFilter6()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log")
			};
			_control.StringFilter = "2";
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().BeEmpty();

			sources.Add(
			            CreateViewModel("test2.log"));
			sources.Add(
			            CreateViewModel("test3.log"));
			_control.FilteredItemsSource.Should().Equal(sources[1]);
		}

		[Test]
		public void TestFilter7()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log"),
				CreateViewModel("test2.log"),
				CreateViewModel("test3.log")
			};
			_control.ItemsSource = sources;

			_control.StringFilter = "3";
			_control.FilteredItemsSource.Should().Equal(sources[2]);
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(sources[1]);
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(sources[0]);
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().BeEmpty();
		}

		[Test]
		public void TestFilter8()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log"),
				CreateViewModel("test2.log"),
				CreateViewModel("test3.log")
			};
			_control.ItemsSource = sources;

			_control.StringFilter = "2";
			_control.FilteredItemsSource.Should().Equal(sources[1]);
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(sources[0]);
			sources.RemoveAt(1);
			_control.FilteredItemsSource.Should().Equal(sources[0]);
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().BeEmpty();
		}

		[Test]
		public void TestFilter9()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log"),
				CreateViewModel("test2.log"),
				CreateViewModel("test3.log")
			};
			_control.ItemsSource = sources;

			_control.StringFilter = "test";
			_control.NoDataSourcesReason.Should().BeNull("because there's three data sources matching the filter");
			_control.NoDataSourcesActions.Should().BeNull("because there's three data sources matching the filter");

			_control.StringFilter = "test4";
			_control.NoDataSourcesReason.Should().Be("No data source matches 'test4'");
			_control.NoDataSourcesActions.Should().Be("Try changing the filter or add the desired data source");

			_control.StringFilter = "test";
			_control.NoDataSourcesReason.Should().BeNull("because there's three data sources matching the filter");
			_control.NoDataSourcesActions.Should().BeNull("because there's three data sources matching the filter");
		}

		[Test]
		public void TestFilterClear()
		{
			var dataSource1 = CreateViewModel("test.log");
			var dataSource2 = CreateViewModel("test2.log");
			var dataSource3 = CreateViewModel("test3.log");
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				dataSource1,
				dataSource2,
				dataSource3
			};
			_control.ItemsSource = sources;

			_control.StringFilter = "test2";
			_control.FilteredItemsSource.Should().Equal(new object[]{dataSource2});

			sources.Clear();
			_control.ItemsSource.Should().BeEmpty();
			_control.FilteredItemsSource.Should().BeEmpty();
		}

		[Test]
		public void TestFilter10()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>();
			_control.ItemsSource = sources;

			_control.StringFilter = "test";
			_control.NoDataSourcesReason.Should().Be("No data source matches 'test'");
			_control.NoDataSourcesActions.Should().Be("Try changing the filter or add the desired data source");
		}

		[Test]
		[Description("Verifies that inserting an item at the first position WITHOUT a filter works")]
		public void TestInsertAt1()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log")
			};
			_control.ItemsSource = sources;
			sources.Insert(0,
			               CreateViewModel("test2.log"));
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		[Description("Verifies that inserting an item at the last position WITHOUT a filter works")]
		public void TestInsertAt2()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test.log")
			};
			_control.ItemsSource = sources;
			sources.Insert(1,
			               CreateViewModel("test2.log"));
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		[Description("Verifies that inserting an item in the middle WITHOUT a filter works")]
		public void TestInsertAt3()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test1.log"),
				CreateViewModel("test2.log")
			};
			_control.ItemsSource = sources;
			sources.Insert(1,
			               CreateViewModel("test3.log"));
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		[Description("Verifies that inserting an item in the middle WITH a filter works")]
		public void TestInsertAt4()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("foo.log"),
				CreateViewModel("test2.log")
			};
			_control.ItemsSource = sources;
			// Let's set a filter that causes the first element to be hidden
			_control.StringFilter = "test";
			sources.Insert(1,
			               CreateViewModel("test3.log"));
			_control.FilteredItemsSource.Should().Equal(new object[] {sources[1], sources[2]});
		}

		[Test]
		[Description("Verifies that inserting an item in the middle WITH a filter works")]
		public void TestInsertAt5()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
			{
				CreateViewModel("test1.log"),
				CreateViewModel("foo.log"),
				CreateViewModel("test2.log")
			};
			_control.ItemsSource = sources;
			// Let's set a filter that causes the first element to be hidden
			_control.StringFilter = "test";
			sources.Insert(2,
			               CreateViewModel("test3.log"));
			_control.FilteredItemsSource.Should().Equal(new object[] {sources[0], sources[2], sources[3]});

			_control.StringFilter = null;
			_control.FilteredItemsSource.Should().Equal(sources);
		}
	}
}