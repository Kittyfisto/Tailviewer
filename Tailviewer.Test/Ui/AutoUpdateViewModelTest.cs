using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class AutoUpdateViewModelTest
	{
		[SetUp]
		public void Setup()
		{
			_updater = new Mock<IAutoUpdater>();
			_settings = new AutoUpdateSettings();
			_dispatcher = new ManualDispatcher();
			_viewModel = new AutoUpdateViewModel(_updater.Object, _settings, _dispatcher);
		}

		private Mock<IAutoUpdater> _updater;
		private AutoUpdateSettings _settings;
		private ManualDispatcher _dispatcher;
		private AutoUpdateViewModel _viewModel;

		[Test]
		public void TestCtor()
		{
			_viewModel.CheckForUpdatesCommand.Should().NotBeNull();
			_viewModel.GotItCommand.Should().NotBeNull();
			_viewModel.InstallCommand.Should().NotBeNull();
		}

		[Test]
		public void TestInstallCommand1()
		{
			_viewModel.InstallCommand.Should().NotBeNull();
			_viewModel.InstallCommand.CanExecute(null).Should().BeTrue();
			_viewModel.InstallTitle.Should().Be("Install");

			_viewModel.InstallCommand.Execute(null);
			_viewModel.InstallCommand.CanExecute(null).Should().BeFalse();
			_viewModel.InstallTitle.Should().Be("Downloading...");
		}

		[Test]
		public void TestInstallCommand2()
		{
			_updater.Verify(x => x.Install(It.IsAny<VersionInfo>()), Times.Never);
			_viewModel.InstallCommand.Execute(null);
			_updater.Verify(x => x.Install(It.IsAny<VersionInfo>()), Times.Once);
		}

		[Test]
		public void TestCheckForUpdates1()
		{
			_viewModel.CheckForUpdatesCommand.CanExecute(null).Should().BeTrue();

			_updater.Verify(x => x.CheckForUpdates(), Times.Never);
			_viewModel.CheckForUpdatesCommand.Execute(null);
			_updater.Verify(x => x.CheckForUpdates(), Times.Once);
		}
	}
}