using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Controls.SidePanel.Issues;

namespace Tailviewer.Test.Ui.Controls.SidePanel.Issues
{
	[TestFixture]
	public sealed class IssuesSidePanelViewModelTest
	{
		private ServiceContainer _services;
		private PluginRegistry _pluginLoader;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_pluginLoader = new PluginRegistry();
			_services.RegisterInstance<IPluginLoader>(_pluginLoader);
		}

		[Test]
		public void TestConstruction()
		{
			var viewModel = new IssuesSidePanelViewModel(_services);
			viewModel.CurrentDataSource.Should().BeNull();
			viewModel.CurrentIssues.Should().BeNull();
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/223")]
		public void TestUnsetCurrentDataSource()
		{
			var viewModel = new IssuesSidePanelViewModel(_services);

			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(new InMemoryLogFile());

			viewModel.CurrentDataSource = dataSource.Object;
			viewModel.CurrentDataSource.Should().Be(dataSource.Object);

			viewModel.CurrentDataSource = null;
			viewModel.CurrentDataSource.Should().BeNull();
		}
	}
}
