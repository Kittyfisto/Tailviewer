using System;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources.Text.Simple;
using Tailviewer.Settings;
using Tailviewer.Test;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.AcceptanceTests.Ui.LogView
{
	[TestFixture]
	public sealed class FileDataSourceViewModelTest
	{
		private DefaultTaskScheduler _taskScheduler;
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _applicationSettings;

		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_actionCenter = new Mock<IActionCenter>();
			_applicationSettings = new Mock<IApplicationSettings>();
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		[Pure]
		private FileDataSourceViewModel CreateFileViewModel(IFileDataSource dataSource)
		{
			return new FileDataSourceViewModel(dataSource, _actionCenter.Object, _applicationSettings.Object);
		}

		[Pure]
		private FolderDataSourceViewModel CreateFolderViewModel(IFolderDataSource dataSource)
		{
			return new FolderDataSourceViewModel(dataSource, _actionCenter.Object, _applicationSettings.Object);
		}

		[Pure]
		private MergedDataSourceViewModel CreateMergedViewModel(IMergedDataSource dataSource)
		{
			return new MergedDataSourceViewModel(dataSource, _actionCenter.Object, _applicationSettings.Object);
		}

		private TextLogSource Create(string fileName)
		{
			return new TextLogSource(_taskScheduler, fileName, LogFileFormats.GenericText, Encoding.Default);
		}

		[Test]
		[Ignore("I broke this one")]
		[LocalTest("AppVeyor doesn't like this test very much")]
		[Description("Verifies that the number of search results is properly forwarded to the view model upon Update()")]
		public void TestSearch1()
		{
			var settings = new DataSource(AbstractTextLogSourceAcceptanceTest.File2Mb) { Id = DataSourceId.CreateNew() };
			using (var logFile = Create(AbstractTextLogSourceAcceptanceTest.File2Mb))
			using (var dataSource = new FileDataSource(_taskScheduler, settings, logFile, TimeSpan.Zero))
			{
				var model = CreateFileViewModel(dataSource);

				logFile.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				model.Property(x =>
				{
					x.Update();
					return x.TotalCount;
				}).ShouldEventually().Be(16114);

				//model.Update();
				//model.TotalCount.Should().Be(16114);

				model.Search.Term = "RPC #12";
				var search = dataSource.Search;
				search.Property(x => x.Count).ShouldEventually().Be(334);

				model.Update();
				model.Search.ResultCount.Should().Be(334);
				model.Search.CurrentResultIndex.Should().Be(0);
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/125")]
		[Description("This is a temporary requirement until #195 is implemented in which case this test must be removed once more")]
		public void TestCannotBeRemovedAsPartOfFolder()
		{
			var dataSource = new Mock<IFileDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());
			var model = CreateFileViewModel(dataSource.Object);
			using (var monitor = model.Monitor())
			{
				model.CanBeRemoved.Should().BeTrue();

				var folderDataSource = new Mock<IFolderDataSource>();
				folderDataSource.Setup(x => x.Settings).Returns(new DataSource());
				var folder = CreateFolderViewModel(folderDataSource.Object);
				model.Parent = folder;
				model.CanBeRemoved.Should().BeFalse();
				monitor.Should().RaisePropertyChangeFor(x => x.CanBeRemoved);

				monitor.Clear();
				model.Parent = null;
				model.CanBeRemoved.Should().BeTrue();
				monitor.Should().RaisePropertyChangeFor(x => x.CanBeRemoved);
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/125")]
		[Description("This is a temporary requirement until #195 is implemented in which case this test must be removed once more")]
		public void TestCanBeRemovedAsPartOfMergedDataSource()
		{
			var actionCenter = new Mock<IActionCenter>();
			var dataSource = new Mock<IFileDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());
			var model = CreateFileViewModel(dataSource.Object);
			using (var monitor = model.Monitor())
			{
				model.CanBeRemoved.Should().BeTrue();

				var mergedDataSource = new Mock<IMergedDataSource>();
				mergedDataSource.Setup(x => x.Settings).Returns(new DataSource());
				var merged = CreateMergedViewModel(mergedDataSource.Object);
				model.Parent = merged;
				model.CanBeRemoved.Should().BeTrue();
				monitor.Should().NotRaisePropertyChangeFor(x => x.CanBeRemoved);

				monitor.Clear();
				model.Parent = null;
				model.CanBeRemoved.Should().BeTrue();
				monitor.Should().NotRaisePropertyChangeFor(x => x.CanBeRemoved);
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/215")]
		public void TestClearAllShowAll()
		{
			var dataSource = new Mock<IFileDataSource>();
			var model = CreateFileViewModel(dataSource.Object);

			model.ScreenCleared.Should().BeFalse();

			model.ClearScreenCommand.Should().NotBeNull();
			model.ClearScreenCommand.CanExecute(null).Should().BeTrue("because the screen can always be cleared");
			model.ShowAllCommand.Should().NotBeNull();
			model.ShowAllCommand.CanExecute(null).Should().BeFalse("because the screen hasn't been cleared so nothing needs to be shown again");
			model.ClearScreenCommand.Execute(null);
			dataSource.Verify(x => x.ClearScreen(), Times.Once);

			model.ShowAllCommand.Should().NotBeNull();
			model.ShowAllCommand.CanExecute(null).Should().BeTrue("because the screen has been cleared and thus everything may be shown again");
			model.ShowAllCommand.Execute(null);
			dataSource.Verify(x => x.ShowAll(), Times.Once);

			model.ShowAllCommand.CanExecute(null).Should().BeFalse("because everything has been shown again and thus nothing further can be shown");
		}
	}
}