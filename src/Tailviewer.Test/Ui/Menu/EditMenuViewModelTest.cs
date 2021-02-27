using System.Linq;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.Menu;

namespace Tailviewer.Test.Ui.Menu
{
	[TestFixture]
	public sealed class EditMenuViewModelTest
	{
		private Mock<ICommand> _goToLineCommand;

		[SetUp]
		public void Setup()
		{
			_goToLineCommand = new Mock<ICommand>();
		}

		[Test]
		public void TestEmpty()
		{
			var menu = new EditMenuViewModel(_goToLineCommand.Object);
			menu.Items.Should().NotBeNull();
			menu.Items.Should().BeEmpty("because there's nothing to go to when there's no data source there");
		}

		[Test]
		public void TestSetUnsetDataSource()
		{
			var menu = new EditMenuViewModel(_goToLineCommand.Object);

			var dataSource = CreateDataSource();
			menu.CurrentDataSource = dataSource;

			menu.Items.Should().NotBeEmpty();
			var goToLine = menu.Items.First(x => x.Header == "Go To Line...");

			menu.CurrentDataSource = null;
			menu.Items.Should().NotContain(goToLine);
			menu.Items.Should().NotContain(x => x.Header == "Go To Line...");
		}

		private IDataSourceViewModel CreateDataSource()
		{
			var dataSource = new Mock<IDataSourceViewModel>();
			return dataSource.Object;
		}
	}
}
