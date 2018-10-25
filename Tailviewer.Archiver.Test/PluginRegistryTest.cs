using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class PluginRegistryTest
	{
		[Test]
		public void TestPlugins()
		{
			var registry = new PluginRegistry();
			registry.Plugins.Should().NotBeNull();
			registry.Plugins.Should().BeEmpty();;
		}

		[Test]
		public void TestLoadEmptyRegistry()
		{
			var registry = new PluginRegistry();
			registry.LoadAllOfType<IWidgetPlugin>().Should().BeEmpty();
			registry.LoadAllOfType<ILogAnalyserPlugin>().Should().BeEmpty();
		}

		[Test]
		public void TestLoadRegisteredPlugin()
		{
			var registry = new PluginRegistry();

			var plugin = CreateWidgetPlugin();
			registry.Register(plugin);

			var plugins = registry.LoadAllOfType<IWidgetPlugin>();
			plugins.Should().NotBeNull();
			plugins.Should().HaveCount(1);
			plugins.First().Should().BeSameAs(plugin);
		}

		private IWidgetPlugin CreateWidgetPlugin()
		{
			var plugin = new Mock<IWidgetPlugin>();
			return plugin.Object;
		}
	}
}
