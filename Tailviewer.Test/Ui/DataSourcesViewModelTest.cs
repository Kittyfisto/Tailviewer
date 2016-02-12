using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
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

			new Action(() => _model.OnDropped(source, dest))
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

			var merged1 = _model.OnDropped(a, b);
			_model.OnDropped(c, a);
			_model.Observable.Should().Equal(new object[] {merged1, d});

			var merged2 = _model.OnDropped(b, d);
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
			_model.OnDropped(a, b);

			var merged2 = _model.OnDropped(b, c);
			a.Parent.Should().BeNull();
			b.Parent.Should().BeSameAs(merged2);
			c.Parent.Should().BeSameAs(merged2);
			merged2.Observable.Should().Equal(new object[] {b, c});
			_model.Observable.Should().Equal(new object[] { a, merged2 });
		}

		[Test]
		[Description("Verifies that dragging a third file onto the group works")]
		public void TestDropOntoGroup1()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");

			_model.OnDropped(a, b);
			var merged = _model.Observable[0] as MergedDataSourceViewModel;
			merged.Should().NotBeNull();

			new Action(() => _model.OnDropped(c, merged)).ShouldNotThrow();
			_model.Observable.Count.Should().Be(1);
			_model.Observable.Should().Equal(new object[] {merged});
			merged.Observable.Should().Equal(new[]
				{
					a, b, c
				});
			c.Parent.Should().BeSameAs(merged);
		}

		[Test]
		[Description("Verifies that dragging a third file onto a file of a group works")]
		public void TestDropOntoGroup2()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");

			var merged = _model.OnDropped(a, b);
			new Action(() => _model.OnDropped(c, b)).ShouldNotThrow();
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

			var merged1 = _model.OnDropped(a, b);
			_model.OnDropped(c, merged1);
			var merged2 = _model.OnDropped(d, e);

			_model.Observable.Should().Equal(new object[]
				{
					merged1,
					merged2
				});
			_model.OnDropped(b, merged2);
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
			_model.OnDropped(a, b);
			_model.CanBeDragged(a).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that a merged source cannot be dragged onto anything")]
		public void TestCanBeDragged3()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var merged = _model.OnDropped(a, b);
			_model.CanBeDragged(merged).Should().BeFalse();
		}

		[Test]
		public void TestCanBeDropped()
		{
			var a = _model.GetOrAdd("A");
			_model.CanBeDropped(a, a).Should().BeFalse("Because an item cannot be dropped onto itself");
		}

		[Test]
		[Description("Verifies that deleting a merged data source puts the original data sources back in place")]
		public void TestRemoveMerged1()
		{
			var a = _model.GetOrAdd("A");
			var b = _model.GetOrAdd("B");
			var c = _model.GetOrAdd("C");
			var d = _model.GetOrAdd("D");

			var merged = _model.OnDropped(b, c);
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

			var merged = _model.OnDropped(a, b);
			a.Parent.Should().BeSameAs(merged);
			b.Parent.Should().BeSameAs(merged);
			merged.RemoveCommand.Execute(null);
			a.Parent.Should().BeNull();
			b.Parent.Should().BeNull();
		}
	}
}