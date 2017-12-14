using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class SingleDataSourceViewModelTest
	{
		private ManualTaskScheduler _scheduler;
		private ILogFileFactory _logFileFactory;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
		}

		[Test]
		public void TestCtor1()
		{
			var settings = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")
				{
					Id = DataSourceId.CreateNew()
			};
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				var model = new SingleDataSourceViewModel(source);
				model.FullName.Should().Be(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test");
				model.Id.Should().Be(settings.Id);

				model.DisplayName.Should().Be("20Mb.test");
				model.CanBeRenamed.Should().BeFalse();
			}
		}

		[Test]
		public void TestCtor2()
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource { Id = DataSourceId.CreateNew(), File = @"C:\temp\foo.txt", SearchTerm = "foobar" }, new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				source.SearchTerm.Should().Be("foobar");

				var model = new SingleDataSourceViewModel(source);
				model.FileName.Should().Be("foo.txt");
				model.SearchTerm.Should().Be("foobar");
			}
		}

		[Test]
		public void TestCtor3([Values(true, false)] bool showDeltaTimes)
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowDeltaTimes = showDeltaTimes
			}, new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				var model = new SingleDataSourceViewModel(source);
				model.ShowDeltaTimes.Should().Be(showDeltaTimes);
			}
		}

		[Test]
		public void TestChangeShowDeltaTimes([Values(true, false)] bool showDeltaTimes)
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowDeltaTimes = showDeltaTimes
			}, new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				var model = new SingleDataSourceViewModel(source);

				var changes = new List<string>();
				model.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);

				model.ShowDeltaTimes = !showDeltaTimes;
				changes.Should().Equal(new object[] {"ShowDeltaTimes"}, "because the property should've changed once");

				model.ShowDeltaTimes = !showDeltaTimes;
				changes.Should().Equal(new object[] { "ShowDeltaTimes" }, "because the property didn't change");

				model.ShowDeltaTimes = showDeltaTimes;
				changes.Should().Equal(new object[] { "ShowDeltaTimes", "ShowDeltaTimes" }, "because the property changed a 2nd time");
			}
		}

		[Test]
		public void TestRename()
		{
			var dataSource = new Mock<ISingleDataSource>();
			dataSource.Setup(x => x.FullFileName).Returns("A:\\foo");
			var model = new SingleDataSourceViewModel(dataSource.Object);

			model.DisplayName.Should().Be("foo");
			new Action(() => model.DisplayName = "bar").ShouldThrow<InvalidOperationException>();
			model.DisplayName.Should().Be("foo");
		}

		[Test]
		public void TestRemoveCommand1()
		{
			using (
				var source =
					new SingleDataSource(_logFileFactory, _scheduler, new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") { Id = DataSourceId.CreateNew() }))
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
					new SingleDataSource(_logFileFactory, _scheduler, new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") { Id = DataSourceId.CreateNew() }))
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
					Id = DataSourceId.CreateNew()
			};
			using (var dataSource = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				var model = new SingleDataSourceViewModel(dataSource);
				var chain = new[] {new SubstringFilter("foobar", true)};
				model.QuickFilterChain = chain;
				model.QuickFilterChain.Should().BeSameAs(chain);
				dataSource.QuickFilterChain.Should().BeSameAs(chain);
			}
		}

		[Test]
		public void TestCharacterCode()
		{
			var dataSource = new Mock<ISingleDataSource>();
			dataSource.SetupAllProperties();

			var model = new SingleDataSourceViewModel(dataSource.Object);
			model.CharacterCode = "ZZ";
			model.CharacterCode.Should().Be("ZZ");
			dataSource.Object.CharacterCode.Should().Be("ZZ");

			model.CharacterCode = "B";
			model.CharacterCode.Should().Be("B");
			dataSource.Object.CharacterCode.Should().Be("B");

			model.CharacterCode = null;
			model.CharacterCode.Should().BeNull();
			dataSource.Object.CharacterCode.Should().BeNull();
		}
	}
}