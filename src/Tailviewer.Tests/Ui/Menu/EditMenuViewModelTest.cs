using System.Linq;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Collections;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;
using Tailviewer.Ui.Menu;

namespace Tailviewer.Tests.Ui.Menu
{
	[TestFixture]
	public sealed class EditMenuViewModelTest
	{
		private Mock<ICommand> _goToLineCommand;
		private Mock<ICommand> _goToDataSource;
		private Mock<ICommand> _goToPreviousDataSource;
		private Mock<ICommand> _goToNextDataSource;
		private Mock<ICommand> _addBookmarkCommand;
		private Mock<ILogViewMainPanelViewModel> _mainPanel;

		[SetUp]
		public void Setup()
		{
			_goToLineCommand = new Mock<ICommand>();
			_goToDataSource = new Mock<ICommand>();
			_goToPreviousDataSource = new Mock<ICommand>();
			_goToNextDataSource = new Mock<ICommand>();
			_mainPanel = new Mock<ILogViewMainPanelViewModel>();
			_addBookmarkCommand = new Mock<ICommand>();
			_mainPanel.Setup(x => x.AddBookmarkCommand).Returns(_addBookmarkCommand.Object);
		}

		[Test]
		public void TestEmpty()
		{
			var menu = CreateMenu();
			menu.Items.Should().NotBeNull();
			menu.Items.Should().BeEmpty("because there's nothing to go to when there's no data source there");
		}

		[Test]
		public void TestSetUnsetDataSource()
		{
			var menu = CreateMenu();

			var dataSource = CreateDataSource();
			menu.CurrentDataSource = dataSource;

			menu.Items.Should().NotBeEmpty();
			menu.Items.Last().Should().NotBeNull();
			var goToLine = menu.Items.First(x => x != null && x.Header == "Go To Line...");
			var goToDataSource = menu.Items.First(x => x != null && x.Header == "Go To Data Source...");
			var goToPreviousDataSource = menu.Items.First(x => x != null && x.Header == "Go To Previous Data Source");
			var goToNextDataSource = menu.Items.First(x => x != null && x.Header == "Go To Next Data Source");
			var addBookmark = menu.Items.First(x => x != null && x.Header == "Add Bookmark");

			menu.CurrentDataSource = null;
			menu.Items.Should().NotContain(goToLine);
			menu.Items.Should().NotContain(goToDataSource);
			menu.Items.Should().NotContain(goToPreviousDataSource);
			menu.Items.Should().NotContain(goToNextDataSource);
			menu.Items.Should().NotContain(addBookmark);
			menu.Items.Should().NotContain(x => x != null && x.Header == "Go To Line...");
			menu.Items.Should().NotContain(x => x != null && x.Header == "Go To Data Source...");
			menu.Items.Should().NotContain(x => x != null && x.Header == "Go To Previous Data Source");
			menu.Items.Should().NotContain(x => x != null && x.Header == "Go To Next Data Source");
			menu.Items.Should().NotContain(x => x != null && x.Header == "Add Bookmark");
		}

		private EditMenuViewModel CreateMenu()
		{
			return new EditMenuViewModel(_goToLineCommand.Object, _goToDataSource.Object, _goToPreviousDataSource.Object, _goToNextDataSource.Object, _mainPanel.Object);
		}

		private IDataSourceViewModel CreateDataSource()
		{
			var dataSource = new Mock<IDataSourceViewModel>();
			dataSource.Setup(x => x.EditMenuItems).Returns(new ObservableCollectionExt<IMenuViewModel>());
			return dataSource.Object;
		}
	}
}
