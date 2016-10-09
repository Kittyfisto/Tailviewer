using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class SingleDataSourceViewModelTest
	{
		private DefaultTaskScheduler _scheduler;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new DefaultTaskScheduler();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
		}

		[Test]
		public void TestCtor1()
		{
			var settings = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")
				{
					Id = Guid.NewGuid()
				};
			using (var source = new SingleDataSource(_scheduler, settings))
			{
				var model = new SingleDataSourceViewModel(source);
				model.FullName.Should().Be(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test");
				model.Id.Should().Be(settings.Id);
			}
		}

		[Test]
		public void TestCtor2()
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource { Id = Guid.NewGuid(), File = @"C:\temp\foo.txt", SearchTerm = "foobar" }, new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				source.SearchTerm.Should().Be("foobar");

				var model = new SingleDataSourceViewModel(source);
				model.FileName.Should().Be("foo.txt");
				model.SearchTerm.Should().Be("foobar");
			}
		}

		[Test]
		public void TestRemoveCommand1()
		{
			using (
				var source =
					new SingleDataSource(_scheduler, new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") { Id = Guid.NewGuid() }))
			{
				var model = new SingleDataSourceViewModel(source);
				model.RemoveCommand.Should().NotBeNull();
				model.RemoveCommand.CanExecute(null).Should().BeTrue();
				new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
			}
		}

		[Test]
		public void TestRemoveCommand2()
		{
			using (
				var source =
					new SingleDataSource(_scheduler, new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") { Id = Guid.NewGuid() }))
			{
				var model = new SingleDataSourceViewModel(source);
				var calls = new List<IDataSourceViewModel>();
				model.Remove += calls.Add;
				new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
				calls.Should().Equal(new object[] {model});
			}
		}

		[Test]
		[Description("Verifies that the quickfilterchain is forwarded to the data source")]
		public void TestSetQuickFilterChain1()
		{
			var settings = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")
				{
					Id = Guid.NewGuid()
				};
			using (var dataSource = new SingleDataSource(_scheduler, settings))
			{
				var model = new SingleDataSourceViewModel(dataSource);
				var chain = new[] {new SubstringFilter("foobar", true)};
				model.QuickFilterChain = chain;
				model.QuickFilterChain.Should().BeSameAs(chain);
				dataSource.QuickFilterChain.Should().BeSameAs(chain);
			}
		}
	}
}