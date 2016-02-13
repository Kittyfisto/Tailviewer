using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.ViewModels;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class DataSourcesViewModelTest
	{
		private ApplicationSettings _settings;
		private DataSources _dataSources;
		private DataSourcesViewModel _model;

		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings("dawddwa");
			_dataSources = new DataSources(_settings.DataSources);
			_model = new DataSourcesViewModel(_settings, _dataSources);
		}

		[Test]
		[Description("Verifies that dropping a source onto another one creates a new group with the 2 sources")]
		public void TestDrop1()
		{
			var source = _model.GetOrAdd("A");
			var dest = _model.GetOrAdd("B");

			new Action(() => _model.OnDropped(source, dest, DataSourceDropType.Group))
				.ShouldNotThrow();
			_model.Observable.Count.Should().Be(1);
			var viewModel = _model.Observable.First();
			viewModel.Should().NotBeNull();
			viewModel.Should().BeOfType<MergedDataSourceViewModel>();
			var merged = (MergedDataSourceViewModel) viewModel;
			merged.Observable.Should().NotBeNull();
			merged.Observable.Should().Equal(new object[]
				{
					source,
					dest
				});

			source.Parent.Should().BeSameAs(merged);
			dest.Parent.Should().BeSameAs(merged);
		}

		[Test]
		[Description("Verifies that dropping a source from one group onto another ungrouped source removes it from the first group and creates a new group with the second source")]
		public void TestDrop2()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			var d = _model.GetOrAdd("D");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged1 = _model.Observable[0] as MergedDataSourceViewModel;
			_model.OnDropped(c, a, DataSourceDropType.Group);
			_model.Observable.Should().Equal(new object[] {merged1, d});

			_model.OnDropped(b, d, DataSourceDropType.Group);
			var merged2 = _model.Observable[1] as MergedDataSourceViewModel;
			b.Parent.Should().BeSameAs(merged2);
			d.Parent.Should().BeSameAs(merged2);
			_model.Observable.Should().Equal(new object[] {merged1, merged2});
		}

		[Test]
		[Description("Verifies that a group is removed if only one source remains in it after a drop")]
		public void TestDrop3()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged1 = _model.Observable[0];
			_model.Observable.Should().Equal(new object[] { merged1, c });

			_model.OnDropped(b, c, DataSourceDropType.Group);
			var merged2 = _model.Observable[1] as MergedDataSourceViewModel;
			a.Parent.Should().BeNull();
			b.Parent.Should().BeSameAs(merged2);
			c.Parent.Should().BeSameAs(merged2);
			merged2.Observable.Should().Equal(new object[] {b, c});
			_model.Observable.Should().Equal(new object[] { a, merged2 });
		}

		[Test]
		[Description("Verifies that the order of data sources is preserved when creating a group")]
		public void TestDrop4()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			var d = _model.GetOrAdd("D");
			var e = _model.GetOrAdd("E");
			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged1 = _model.Observable[0] as MergedDataSourceViewModel;
			_model.Observable.Should().Equal(new object[] { merged1, c, d, e });
		}

		[Test]
		[Description("Verifies that the order of data sources is preserved when creating a group")]
		public void TestDrop5()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			var d = _model.GetOrAdd("D");
			var e = _model.GetOrAdd("E");
			_model.OnDropped(a, e, DataSourceDropType.Group);
			var merged1 = _model.Observable[3] as MergedDataSourceViewModel;
			_model.Observable.Should().Equal(new object[] { b, c, d, merged1 });
		}

		[Test]
		[Description("Verifies that arranging a source to the topmost spot works")]
		public void TestRearrange1()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");

			new Action(() => _model.OnDropped(b, a, DataSourceDropType.ArrangeTop))
				.ShouldNotThrow();
			_model.Observable.Should().Equal(new object[]
				{
					b, a
				});

			a.Parent.Should().BeNull();
			b.Parent.Should().BeNull();
		}

		[Test]
		[Description("Verifies that arranging a source to the bottom-most spot works")]
		public void TestRearrange2()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");

			new Action(() => _model.OnDropped(a, b, DataSourceDropType.ArrangeBottom))
				.ShouldNotThrow();
			_model.Observable.Should().Equal(new object[]
				{
					b, a
				});

			a.Parent.Should().BeNull();
			b.Parent.Should().BeNull();
		}

		[Test]
		[Description("Verifies that arranging a source from a group destroys the group when there's only 1 source left")]
		public void TestRearrange3()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var group = _model.Observable[0] as MergedDataSourceViewModel;
			_model.Observable.Should().Equal(new[]
				{
					group, c
				});

			new Action(() => _model.OnDropped(b, c, DataSourceDropType.ArrangeBottom))
				.ShouldNotThrow();
			_model.Observable.Should().Equal(new object[]
				{
					a, c, b
				});

			a.Parent.Should().BeNull();
			b.Parent.Should().BeNull();
			c.Parent.Should().BeNull();
		}

		[Test]
		[Description("Verifies that re-arranging groups is possible")]
		public void TestRearrange4()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			var d = _model.GetOrAdd("D");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			_model.OnDropped(c, d, DataSourceDropType.Group);
			var group1 = _model.Observable[0] as MergedDataSourceViewModel;
			var group2 = _model.Observable[1] as MergedDataSourceViewModel;

			new Action(() => _model.OnDropped(group1, group2, DataSourceDropType.ArrangeBottom))
				.ShouldNotThrow();
			_model.Observable.Should().Equal(new object[]
				{
					group2, group1
				});
		}

		[Test]
		[Description("Verifies that dropping and rearranging an item inside a group works")]
		public void TestRearrange5()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var group = _model.Observable[0] as MergedDataSourceViewModel;

			new Action(() => _model.OnDropped(c, a, DataSourceDropType.ArrangeBottom | DataSourceDropType.Group))
				.ShouldNotThrow();
			_model.Observable.Should().Equal(new object[]
				{
					group
				});

			group.Observable.Should().Equal(new object[]
				{
					a, c, b
				});
			c.Parent.Should().BeSameAs(group);
		}

		[Test]
		[Description("Verifies that rearranging an item inside a group works")]
		public void TestRearrange6()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var group = _model.Observable[0] as MergedDataSourceViewModel;
			group.Observable.Should().Equal(new object[] { a, b });

			new Action(() => _model.OnDropped(a, b, DataSourceDropType.ArrangeBottom)).ShouldNotThrow();
			_model.Observable.Should().Equal(new object[] {group});
			group.Observable.Should().Equal(new object[] {b, a});
		}

		[Test]
		[Description("Verifies that dragging a third file onto the group works")]
		public void TestDropOntoGroup1()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged = _model.Observable[0] as MergedDataSourceViewModel;
			merged.Should().NotBeNull();

			new Action(() => _model.OnDropped(c, merged, DataSourceDropType.Group)).ShouldNotThrow();
			_model.Observable.Count.Should().Be(1);
			_model.Observable.Should().Equal(new object[] {merged});
			merged.Observable.Should().Equal(new[] {a, b, c});
			c.Parent.Should().BeSameAs(merged);
		}

		[Test]
		[Description("Verifies that dragging a third file onto a file of a group works")]
		public void TestDropOntoGroup2()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged = _model.Observable[0] as MergedDataSourceViewModel;
			new Action(() => _model.OnDropped(c, b, DataSourceDropType.Group)).ShouldNotThrow();
			_model.Observable.Count.Should().Be(1);
			_model.Observable.Should().Equal(new object[] { merged });
			merged.Observable.Should().Equal(new[]
				{
					a, b, c
				});
			c.Parent.Should().BeSameAs(merged);
		}

		[Test]
		[Description("Verifies that dragging a file from one group onto another works")]
		public void TestDropOntoGroup3()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			var d = _model.GetOrAdd("D");
			var e = _model.GetOrAdd("E");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged1 = _model.Observable[0] as MergedDataSourceViewModel;
			_model.OnDropped(c, merged1, DataSourceDropType.Group);
			_model.OnDropped(d, e, DataSourceDropType.Group);
			var merged2 = _model.Observable[1] as MergedDataSourceViewModel;

			_model.Observable.Should().Equal(new object[]
				{
					merged1,
					merged2
				});
			_model.OnDropped(b, merged2, DataSourceDropType.Group);
			b.Parent.Should().BeSameAs(merged2);

			merged1.Observable.Should().Equal(new object[] { a, c });
			merged2.Observable.Should().Equal(new object[] { d, e, b });
		}

		[Test]
		[Description("Verifies that a single source can be dragged")]
		public void TestCanBeDragged1()
		{
			var a = _model.GetOrAdd("A");
			_model.CanBeDragged(a).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that a source with a parent can be dragged")]
		public void TestCanBeDragged2()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			_model.OnDropped(a, b, DataSourceDropType.Group);
			_model.CanBeDragged(a).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that a merged source cannot be dragged to form a group")]
		public void TestCanBeDragged3()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged = _model.Observable[0] as MergedDataSourceViewModel;
			IDataSourceViewModel unused;
			_model.CanBeDropped(merged, c, DataSourceDropType.Group, out unused).Should().BeFalse();
		}

		[Test]
		public void TestCanBeDropped1()
		{
			var a = _model.GetOrAdd("A");
			IDataSourceViewModel unused;
			_model.CanBeDropped(a, a, DataSourceDropType.Group, out unused).Should().BeFalse("Because an item cannot be dropped onto itself");
		}

		[Test]
		public void TestCanBeDropped2()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			IDataSourceViewModel unused;
			_model.CanBeDropped(a, b, DataSourceDropType.Group, out unused).Should().BeTrue("Because two items can be grouped");
		}

		[Test]
		[Description("Verifies that an item can be dropped above an item in a group")]
		public void TestCanBeDropped3()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			_model.OnDropped(a, b, DataSourceDropType.Group);

			IDataSourceViewModel group;
			_model.CanBeDropped(c, a,
				DataSourceDropType.Group | DataSourceDropType.ArrangeTop, out group).Should().BeTrue();
			group.Should().BeSameAs(_model.Observable[0]);
		}

		[Test]
		[Description("Verifies that an item can be dropped below an item in a group")]
		public void TestCanBeDropped4()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			_model.OnDropped(a, b, DataSourceDropType.Group);

			IDataSourceViewModel group;
			_model.CanBeDropped(c, a,
				DataSourceDropType.Group | DataSourceDropType.ArrangeBottom, out group).Should().BeTrue();
			group.Should().BeSameAs(_model.Observable[0]);
		}

		[Test]
		[Description("Verifies that deleting a merged data source puts the original data sources back in place")]
		public void TestRemoveMerged1()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			var d = _model.GetOrAdd("D");

			_model.OnDropped(b, c, DataSourceDropType.Group);
			var merged = _model.Observable[1] as MergedDataSourceViewModel;
			_model.Observable.Should().Equal(new[]
				{
					a, merged, d
				});
			merged.RemoveCommand.Execute(null);
			_model.Observable.Should().Equal(new[]
				{
					a, b, c, d
				});
		}

		[Test]
		[Description("Verifies that deleting a merged data source removes its as parent from its child data sources")]
		public void TestRemoveMerged2()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");

			_model.OnDropped(a, b, DataSourceDropType.Group);
			var merged = _model.Observable[0] as MergedDataSourceViewModel;
			a.Parent.Should().BeSameAs(merged);
			b.Parent.Should().BeSameAs(merged);
			merged.RemoveCommand.Execute(null);
			a.Parent.Should().BeNull();
			b.Parent.Should().BeNull();
		}
	}
}