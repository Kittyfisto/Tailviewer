using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Filters;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;
using Tailviewer.Ui.ViewModels.ContextMenu;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class FileDataSourceViewModelTest
	{
		private ILogFileFactory _logFileFactory;
		private ManualTaskScheduler _scheduler;
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _settings;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
			_actionCenter = new Mock<IActionCenter>();
			_settings = new Mock<IApplicationSettings>();
		}

		[Pure]
		private FileDataSourceViewModel CreateFileViewModel(IFileDataSource dataSource)
		{
			return new FileDataSourceViewModel(dataSource, _actionCenter.Object, _settings.Object);
		}

		[Pure]
		private FolderDataSourceViewModel CreateFolderViewModel(IFolderDataSource dataSource)
		{
			return new FolderDataSourceViewModel(dataSource, _actionCenter.Object, _settings.Object);
		}

		[Test]
		public void TestConstruction1()
		{
			var settings = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")
				{
					Id = DataSourceId.CreateNew()
			};
			using (var source = new FileDataSource(_logFileFactory, _scheduler, settings))
			{
				var model = CreateFileViewModel(source);
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
				var source = new FileDataSource(_scheduler,
					new DataSource {Id = DataSourceId.CreateNew(), File = @"C:\temp\foo.txt", SearchTerm = "foobar"},
					new Mock<ILogSource>().Object, TimeSpan.Zero))
			{
				source.SearchTerm.Should().Be("foobar");

				var model = CreateFileViewModel(source);
				model.Search.Term.Should().Be("foobar");
			}
		}

		[Test]
		public void TestConstruction3([Values(true, false)] bool showDeltaTimes)
		{
			using (var source = new FileDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowDeltaTimes = showDeltaTimes
			}, new Mock<ILogSource>().Object, TimeSpan.Zero))
			{
				var model = CreateFileViewModel(source);
				model.ShowDeltaTimes.Should().Be(showDeltaTimes);
			}
		}

		[Test]
		public void TestConstruction4([Values(true, false)] bool showElapsedTime)
		{
			using (var source = new FileDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowElapsedTime = showElapsedTime
			}, new Mock<ILogSource>().Object, TimeSpan.Zero))
			{
				var model = CreateFileViewModel(source);
				model.ShowElapsedTime.Should().Be(showElapsedTime);
			}
		}

		[Test]
		public void TestChangeShowElapsedTime([Values(true, false)] bool showElapsedTime)
		{
			using (var source = new FileDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowElapsedTime = showElapsedTime
			}, new Mock<ILogSource>().Object, TimeSpan.Zero))
			{
				var model = CreateFileViewModel(source);

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
			using (var source = new FileDataSource(_scheduler, new DataSource
			{
				Id = DataSourceId.CreateNew(),
				File = @"C:\temp\foo.txt",
				ShowDeltaTimes = showDeltaTimes
			}, new Mock<ILogSource>().Object, TimeSpan.Zero))
			{
				var model = CreateFileViewModel(source);

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
			var dataSource = new Mock<IFileDataSource>();
			dataSource.Setup(x => x.FullFileName).Returns("A:\\foo");
			var model = CreateFileViewModel(dataSource.Object);

			model.DisplayName.Should().Be("foo");
			new Action(() => model.DisplayName = "bar").Should().Throw<InvalidOperationException>();
			model.DisplayName.Should().Be("foo");
		}

		[Test]
		public void TestRemoveCommand1()
		{
			using (
				var source =
					new FileDataSource(_logFileFactory, _scheduler,
						new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") {Id = DataSourceId.CreateNew()}))
			{
				var model = CreateFileViewModel(source);
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
					new FileDataSource(_logFileFactory, _scheduler,
						new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test") {Id = DataSourceId.CreateNew()}))
			{
				var model = CreateFileViewModel(source);
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
			using (var dataSource = new FileDataSource(_logFileFactory, _scheduler, settings))
			{
				var model = CreateFileViewModel(dataSource);
				var chain = new[] {new SubstringFilter("foobar", true)};
				model.QuickFilterChain = chain;
				model.QuickFilterChain.Should().BeSameAs(chain);
				dataSource.QuickFilterChain.Should().BeSameAs(chain);
			}
		}

		[Test]
		public void TestCharacterCode()
		{
			var dataSource = new Mock<IFileDataSource>();
			dataSource.SetupAllProperties();

			var model = CreateFileViewModel(dataSource.Object);
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
			var single = new Mock<IFileDataSource>();
			single.Setup(x => x.Settings).Returns(new DataSource());
			single.Setup(x => x.FullFileName).Returns(@"C:\Users\Simon\AppData\Local\Tailviewer\Installation.log");
			var singleViewModel = CreateFileViewModel(single.Object);
			using (var monitor = singleViewModel.Monitor())
			{
				singleViewModel.DisplayName.Should().Be("Installation.log");
				singleViewModel.Folder.Should().Be(@"C:\Users\Simon\AppData\Local\Tailviewer\");

				var folder = new Mock<IFolderDataSource>();
				folder.Setup(x => x.OriginalSources).Returns(new List<IDataSource>());
				folder.Setup(x => x.LogFileFolderPath).Returns(@"C:\Users\Simon\AppData\Local\");
				var folderViewModel = CreateFolderViewModel(folder.Object);

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
			var single = new Mock<IFileDataSource>();
			single.Setup(x => x.Settings).Returns(new DataSource());
			var pluginDescription = new PluginDescription();
			single.Setup(x => x.TranslationPlugin).Returns(pluginDescription);
			var singleViewModel = CreateFileViewModel(single.Object);
			singleViewModel.TranslationPlugin.Should().NotBeNull();
			singleViewModel.TranslationPlugin.Should().BeSameAs(pluginDescription);
		}

		[Test]
		[Description("Verifies that the description of the context menu item changes even when the ExcludeFromParent property is updated on its own")]
		public void TestToggleExcludeViaModel()
		{
			var dataSource = new Mock<IFileDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());
			dataSource.Setup(x => x.FullFileName).Returns("A:\\foo");
			var model = CreateFileViewModel(dataSource.Object);
			var parent = new Mock<IMergedDataSourceViewModel>();
			var parentDataSource = new Mock<IMultiDataSource>();
			parentDataSource.Setup(x => x.Id).Returns(DataSourceId.CreateNew());
			parent.Setup(x => x.DataSource).Returns(parentDataSource.Object);

			model.Parent = parent.Object;
			model.ContextMenuItems.Should().HaveCount(1);
			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Header.Should().Be("Exclude from group");

			model.ExcludeFromParent = true;
			parentDataSource.Verify(x => x.SetExcluded(dataSource.Object, true), Times.Once);
			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Header.Should().Be("Include in group");

			model.ExcludeFromParent = false;
			parentDataSource.Verify(x => x.SetExcluded(dataSource.Object, false), Times.Once);
			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Header.Should().Be("Exclude from group");
		}

		[Test]
		public void TestToggleExcludeViaContextMenu()
		{
			var dataSource = new Mock<IFileDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());
			dataSource.Setup(x => x.FullFileName).Returns("A:\\foo");
			var model = CreateFileViewModel(dataSource.Object);
			var parent = new Mock<IMergedDataSourceViewModel>();
			var parentDataSource = new Mock<IMultiDataSource>();
			parentDataSource.Setup(x => x.Id).Returns(DataSourceId.CreateNew());
			parent.Setup(x => x.DataSource).Returns(parentDataSource.Object);

			model.Parent = parent.Object;
			model.ContextMenuItems.Should().HaveCount(1);
			model.ExcludeFromParent.Should().BeFalse();
			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Header.Should().Be("Exclude from group");

			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Command.Execute(null);
			model.ExcludeFromParent.Should().BeTrue();
			parentDataSource.Verify(x => x.SetExcluded(dataSource.Object, true), Times.Once);
			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Header.Should().Be("Include in group");

			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Command.Execute(null);
			model.ExcludeFromParent.Should().BeFalse();
			parentDataSource.Verify(x => x.SetExcluded(dataSource.Object, false), Times.Once);
			((ToggleExcludeFromGroupViewModel) model.ContextMenuItems.First()).Header.Should().Be("Exclude from group");
		}
	}
}