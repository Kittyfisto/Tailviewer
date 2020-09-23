using System;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel.Settings;

namespace Tailviewer.Test.Ui.Controls.MainPanel.Settings
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class SettingsControlTest
	{
		private Mock<IApplicationSettings> _applicationSettings;
		private Mock<IAutoUpdateSettings> _settings;
		private SettingsControl _control;

		[SetUp]
		public void Setup()
		{
			_applicationSettings = new Mock<IApplicationSettings>();
			_settings = new Mock<IAutoUpdateSettings>();
			_settings.SetupAllProperties();
			_applicationSettings.Setup(x => x.AutoUpdate).Returns(_settings.Object);
			_applicationSettings.Setup(x => x.LogViewer).Returns(new Mock<ILogViewerSettings>().Object);
			_control = new SettingsControl();
			_control.ProxyPasswordBox.ApplyTemplate();
		}

		[Test]
		public void TestChangeDataContext()
		{
			_settings.Object.ProxyPassword = "1234";
			var dataContext = new SettingsMainPanelViewModel(_applicationSettings.Object, new ServiceContainer());
			_control.DataContext = dataContext;

			new Action(() => _control.DataContext = null).Should().NotThrow();
		}
	}
}