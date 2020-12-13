using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.ActionCenter;
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
		private ILogFileFactory _logFileFactory;
		private ManualTaskScheduler _scheduler;
		private Mock<IActionCenter> _actionCenter;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
			_actionCenter = new Mock<IActionCenter>();
		}

		[Test]
		public void TestConstruction1()
		{
			var settings = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")
				{
					Id = DataSourceId.CreateNew()
			};
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);
				model.FullName.Should().Be(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test");
				model.Id.Should().Be(settings.Id);

				model.DisplayName.Should().Be("20Mb.test");
				model.CanBeRenamed.Should().BeFalse();
			}
		}

		[Test]
		public void TestConstruction2()
		{
			using (
				var source = new SingleDataSource(_scheduler,
					new DataSource {Id = DataSourceId.CreateNew(), File = @"C:\temp\foo.txt", SearchTerm = "foobar"},
					new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				source.SearchTerm.Should().Be("foobar");

				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);
				model.SearchTerm.Should().Be("foobar");
			}
		}

		[Test]
		public void TestConstruction3([Values(true, false)] bool showDeltaTimes)
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowDeltaTimes = showDeltaTimes
			}, new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);
				model.ShowDeltaTimes.Should().Be(showDeltaTimes);
			}
		}

		[Test]
		public void TestConstruction4([Values(true, false)] bool showElapsedTime)
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowElapsedTime = showElapsedTime
			}, new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);
				model.ShowElapsedTime.Should().Be(showElapsedTime);
			}
		}

		[Test]
		public void TestChangeShowElapsedTime([Values(true, false)] bool showElapsedTime)
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowElapsedTime = showElapsedTime
			}, new Mock<ILogFile>().Object, TimeSpan.Zero))
			{
				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);

				var changes = new List<string>();
				model.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);

				model.ShowElapsedTime = !showElapsedTime;
				changes.Should().Equal(new object[] {"ShowElapsedTime"}, "because the property should've changed once");

				model.ShowElapsedTime = !showElapsedTime;
				changes.Should().Equal(new object[] { "ShowElapsedTime" }, "because the property didn't change");

				model.ShowElapsedTime = showElapsedTime;
				changes.Should().Equal(new object[] { "ShowElapsedTime", "ShowElapsedTime" }, "because the property changed a 2nd time");
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
				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);

				var changes = new List<string>();
				model.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);

				model.ShowDeltaTimes = !showDeltaTimes;
				changes.Should().Equal(new object[] {"ShowDeltaTimes"}, "because the property should've changed once");

				model.ShowDeltaTimes = !showDeltaTimes;
				changes.Should().Equal(new object[] {"ShowDeltaTimes"}, "because the property didn't change");

				model.ShowDeltaTimes = showDeltaTimes;
				changes.Should().Equal(new object[] {"ShowDeltaTimes", "ShowDeltaTimes"}, "because the property changed a 2nd time");
			}
		}

		[Test]
		public void TestRename()
		{
			var dataSource = new Mock<ISingleDataSource>();
			dataSource.Setup(x => x.FullFileName).Returns("A:\\foo");
			var model = new SingleDataSourceViewModel(dataSource.Object, _actionCenter.Object);

			model.DisplayName.Should().Be("foo");
			new Action(() => model.DisplayName = "bar").Should().Throw<InvalidOperationException>();
			model.DisplayName.Should().Be("foo");
		}

		[Test]
		public void TestRemoveCommand1()
		{
			using (
				var source =
					new SingleDataSource(_logFileFactory, _scheduler,
						new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") {Id = DataSourceId.CreateNew()}))
			{
				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);
				model.RemoveCommand.Should().NotBeNull();
				model.RemoveCommand.CanExecute(null).Should().BeTrue();
				new Action(() => model.RemoveCommand.Execute(null)).Should().NotThrow();
			}
		}

		[Test]
		public void TestRemoveCommand2()
		{
			using (
				var source =
					new SingleDataSource(_logFileFactory, _scheduler,
						new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") {Id = DataSourceId.CreateNew()}))
			{
				var model = new SingleDataSourceViewModel(source, _actionCenter.Object);
				var calls = new List<IDataSourceViewModel>();
				model.Remove += calls.Add;
				new Action(() => model.RemoveCommand.Execute(null)).Should().NotThrow();
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
				var model = new SingleDataSourceViewModel(dataSource, _actionCenter.Object);
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

			var model = new SingleDataSourceViewModel(dataSource.Object, _actionCenter.Object);
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

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/125")]
		public void TestDisplayRelativePathWithFolderParent()
		{
			var single = new Mock<ISingleDataSource>();
			single.Setup(x => x.Settings).Returns(new DataSource());
			single.Setup(x => x.FullFileName).Returns(@"C:\Users\Simon\AppData\Local\Tailviewer\Installation.log");
			var singleViewModel = new SingleDataSourceViewModel(single.Object, _actionCenter.Object);
			using (var monitor = singleViewModel.Monitor())
			{
				singleViewModel.DisplayName.Should().Be("Installation.log");
				singleViewModel.Folder.Should().Be(@"C:\Users\Simon\AppData\Local\Tailviewer\");

				var folder = new Mock<IFolderDataSource>();
				folder.Setup(x => x.OriginalSources).Returns(new List<IDataSource>());
				folder.Setup(x => x.LogFileFolderPath).Returns(@"C:\Users\Simon\AppData\Local\");
				var folderViewModel = new FolderDataSourceViewModel(folder.Object, _actionCenter.Object);

				singleViewModel.Parent = folderViewModel;
				singleViewModel.DisplayName.Should().Be("Installation.log");
				singleViewModel.Folder.Should().Be(@"<root>\Tailviewer\");
				monitor.Should().RaisePropertyChangeFor(x => x.Folder);
				monitor.Clear();

				singleViewModel.Parent = null;
				singleViewModel.DisplayName.Should().Be("Installation.log");
				singleViewModel.Folder.Should().Be(@"C:\Users\Simon\AppData\Local\Tailviewer\");
				monitor.Should().RaisePropertyChangeFor(x => x.Folder);
			}
		}

		[Test]
		public void TestPluginDescription()
		{
			var single = new Mock<ISingleDataSource>();
			single.Setup(x => x.Settings).Returns(new DataSource());
			var pluginDescription = new PluginDescription();
			single.Setup(x => x.TranslationPlugin).Returns(pluginDescription);
			var singleViewModel = new SingleDataSourceViewModel(single.Object, _actionCenter.Object);
			singleViewModel.TranslationPlugin.Should().NotBeNull();
			singleViewModel.TranslationPlugin.Should().BeSameAs(pluginDescription);
		}
	}
}