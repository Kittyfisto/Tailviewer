using System.Threading;
using System.Windows;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Plugins;
using Tailviewer.Ui;
using Tailviewer.Ui.Controls.SidePanel.Outline;

namespace Tailviewer.Test.Ui.Controls.SidePanel.Outline
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class OutlineSidePanelViewModelTest
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
			var viewModel = new OutlineSidePanelViewModel(_services);
			viewModel.CurrentDataSource.Should().BeNull();
			viewModel.CurrentContent.Should().BeNull();
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/223")]
		public void TestUnsetCurrentDataSource()
		{
			var viewModel = new OutlineSidePanelViewModel(_services);

			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(new InMemoryLogSource());

			viewModel.CurrentDataSource = dataSource.Object;
			viewModel.CurrentDataSource.Should().Be(dataSource.Object);

			viewModel.CurrentDataSource = null;
			viewModel.CurrentDataSource.Should().BeNull();
		}

		[Test]
		public void TestMultiSource()
		{
			var plugin = new Mock<ILogFileOutlinePlugin>();
			plugin.Setup(x => x.SupportedFormats).Returns(new[] {LogFileFormats.ExtendedLogFormat});
			plugin.Setup(x => x.CreateViewModel(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			      .Returns(new Mock<ILogFileOutlineViewModel>().Object);

			var pluginContent = new FrameworkElement();
			plugin.Setup(x => x.CreateContentPresenterFor(It.IsAny<IServiceContainer>(),
			                                              It.IsAny<ILogFileOutlineViewModel>()))
			      .Returns(pluginContent);
			_pluginLoader.Register(plugin.Object);

			var viewModel = new OutlineSidePanelViewModel(_services);

			var dataSource = new Mock<IMultiDataSource>();
			var logFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(new[]{CreateDataSource(LogFileFormats.CommonLogFormat), CreateDataSource(LogFileFormats.ExtendedLogFormat)});

			viewModel.CurrentContent.Should().BeNull();
			viewModel.CurrentDataSource.Should().BeNull();

			viewModel.CurrentDataSource = dataSource.Object;
			plugin.Verify(x => x.CreateViewModel(_services, logFile.Object), Times.Once);
			viewModel.CurrentContent.Should().BeSameAs(pluginContent);
		}

		private IDataSource CreateDataSource(ILogFileFormat format)
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.Format)).Returns(format);
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			return dataSource.Object;
		}
	}
}
