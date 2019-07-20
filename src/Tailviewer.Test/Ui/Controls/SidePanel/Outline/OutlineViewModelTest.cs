using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Ui.Controls.SidePanel.Outline;
using Tailviewer.Ui.Outline;

namespace Tailviewer.Test.Ui.Controls.SidePanel.Outline
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class OutlineViewModelTest
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
		public void TestMultiSource()
		{
			var plugin = new Mock<ILogFileOutlinePlugin>();
			plugin.Setup(x => x.SupportedFileNames).Returns(new[] {new Regex(@"Apache\.log"),});
			plugin.Setup(x => x.CreateViewModel(It.IsAny<IServiceContainer>(), It.IsAny<ILogFile>()))
			      .Returns(new Mock<ILogFileOutlineViewModel>().Object);

			var pluginContent = new FrameworkElement();
			plugin.Setup(x => x.CreateContentPresenterFor(It.IsAny<IServiceContainer>(),
			                                              It.IsAny<ILogFileOutlineViewModel>()))
			      .Returns(pluginContent);
			_pluginLoader.Register(plugin.Object);

			var viewModel = new OutlineViewModel(_services);

			var dataSource = new Mock<IMultiDataSource>();
			var logFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(new[]{CreateDataSource(fileName: "Foo"), CreateDataSource(fileName: "Apache.log")});

			viewModel.CurrentContent.Should().BeNull();
			viewModel.CurrentDataSource.Should().BeNull();

			viewModel.CurrentDataSource = dataSource.Object;
			plugin.Verify(x => x.CreateViewModel(_services, logFile.Object), Times.Once);
			viewModel.CurrentContent.Should().BeSameAs(pluginContent);
		}

		private IDataSource CreateDataSource(string fileName)
		{
			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.FullFileName).Returns(fileName);
			return dataSource.Object;
		}
	}
}
