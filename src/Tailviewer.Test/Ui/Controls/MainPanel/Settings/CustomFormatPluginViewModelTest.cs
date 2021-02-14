using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.LogFileFormats;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Settings.CustomFormats;
using Tailviewer.Ui.Controls.MainPanel.Settings;
using Tailviewer.Ui.Controls.MainPanel.Settings.CustomFormats;

namespace Tailviewer.Test.Ui.Controls.MainPanel.Settings
{
	[TestFixture]
	public sealed class CustomFormatPluginViewModelTest
	{
		private ServiceContainer _serviceContainer;
		private Mock<ILogFileFormatRegistry> _registry;
		private Mock<IApplicationSettings> _settings;
		private CustomFormatsSettings _customFormatsSettings;
		private PluginId _pluginId;
		private Mock<ICustomLogFileFormatCreatorPlugin> _plugin;
		private IEnumerable<EncodingViewModel> _encodings;

		[SetUp]
		public void Setup()
		{
			_serviceContainer = new ServiceContainer();

			_registry = new Mock<ILogFileFormatRegistry>();
			_serviceContainer.RegisterInstance(_registry.Object);

			_settings = new Mock<IApplicationSettings>();
			_customFormatsSettings = new CustomFormatsSettings();
			_settings.SetupGet(x => x.CustomFormats).Returns(_customFormatsSettings);

			_pluginId = new PluginId("221dwdwaddwa");
			_plugin = new Mock<ICustomLogFileFormatCreatorPlugin>();
			_encodings = new[] {new EncodingViewModel(Encoding.Default)};
		}

		private CustomFormatPluginViewModel Create()
		{
			return new CustomFormatPluginViewModel(_serviceContainer, _settings.Object, _pluginId, _plugin.Object, _encodings);
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/278")]
		public void TestAddRemove()
		{
			var viewModel = Create();
			viewModel.Formats.Should().BeEmpty();

			viewModel.AddCommand.Execute(null);
			viewModel.Formats.Should().HaveCount(1);
			_customFormatsSettings.Should().HaveCount(1);

			var format = viewModel.Formats.First();
			format.RemoveCommand.Execute(null);
			viewModel.Formats.Should().BeEmpty("because the format should have been removed again");
			_customFormatsSettings.Should().BeEmpty("because the format should have been removed again");
		}
	}
}
