using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels;

namespace Tailviewer.Test.Ui.Controls.Analyse.SidePanels
{
	[TestFixture]
	public sealed class WidgetsSidePanelTest
	{
		private PluginRegistry _pluginRegistry;

		[SetUp]
		public void Setup()
		{
			_pluginRegistry = new PluginRegistry();
		}

		[Test]
		public void TestConstruction()
		{
			_pluginRegistry.Register(CreatePlugin());
			_pluginRegistry.Register(CreatePlugin());
			_pluginRegistry.Register(CreatePlugin());
			var sidePanel = new WidgetsSidePanel(_pluginRegistry);
			sidePanel.Widgets.Should().HaveCount(3, "because every of the three registered widgets should be represented by a view model");
		}

		private IWidgetPlugin CreatePlugin()
		{
			var plugin = new Mock<IWidgetPlugin>();
			return plugin.Object;
		}
	}
}
