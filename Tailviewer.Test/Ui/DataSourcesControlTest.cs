using System;
using System.Collections.ObjectModel;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;
using DataSource = Tailviewer.Settings.DataSource;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class DataSourcesControlTest
	{
		private DataSourcesControl _control;

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_control = new DataSourcesControl();
		}

		[Test]
		[STAThread]
		public void TestFilter1()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>();
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().BeEmpty();
		}

		[Test]
		[STAThread]
		public void TestFilter2()
		{
			var sources = new ObservableCollection<IDataSourceViewModel>
				{
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test.log")))
				};
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		[STAThread]
		public void TestFilter3()
		{
			var sources = new ObservableCollection<SingleDataSourceViewModel>();
			_control.ItemsSource = sources;

			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test.log"))));
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test2.log"))));
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test3.log"))));
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		[STAThread]
		public void TestFilter4()
		{
			var sources = new ObservableCollection<SingleDataSourceViewModel>();
			_control.ItemsSource = sources;

			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test.log"))));
			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test2.log"))));
			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test3.log"))));

			sources.RemoveAt(1);
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(sources);

			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(sources);
		}

		[Test]
		[STAThread]
		public void TestFilter5()
		{
			var sources = new ObservableCollection<SingleDataSourceViewModel>
				{
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test.log"))),
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test2.log"))),
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test3.log")))
				};
			_control.StringFilter = "2";
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().Equal(new[] {sources[1]});
		}

		[Test]
		[STAThread]
		public void TestFilter6()
		{
			var sources = new ObservableCollection<SingleDataSourceViewModel>
				{
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test.log"))),
				};
			_control.StringFilter = "2";
			_control.ItemsSource = sources;
			_control.FilteredItemsSource.Should().BeEmpty();

			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test2.log"))));
			sources.Add(new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test3.log"))));
			_control.FilteredItemsSource.Should().Equal(new[] { sources[1] });
		}

		[Test]
		[STAThread]
		public void TestFilter7()
		{
			var sources = new ObservableCollection<SingleDataSourceViewModel>
				{
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test.log"))),
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test2.log"))),
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test3.log")))
				};
			_control.ItemsSource = sources;

			_control.StringFilter = "3";
			_control.FilteredItemsSource.Should().Equal(new[] { sources[2] });
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(new[] { sources[1] });
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(new[] { sources[0] });
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().BeEmpty();
		}

		[Test]
		[STAThread]
		public void TestFilter8()
		{
			var sources = new ObservableCollection<SingleDataSourceViewModel>
				{
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test.log"))),
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test2.log"))),
					new SingleDataSourceViewModel(new Tailviewer.BusinessLogic.SingleDataSource(new DataSource("test3.log")))
				};
			_control.ItemsSource = sources;

			_control.StringFilter = "2";
			_control.FilteredItemsSource.Should().Equal(new[] { sources[1] });
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().Equal(new[] { sources[0] });
			sources.RemoveAt(1);
			_control.FilteredItemsSource.Should().Equal(new[] { sources[0] });
			sources.RemoveAt(0);
			_control.FilteredItemsSource.Should().BeEmpty();
		}
	}
}