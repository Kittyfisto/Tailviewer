using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class PluginCacheTest
	{
		private Mock<IPluginLoader> _pluginLoader;

		[SetUp]
		public void Setup()
		{
			_pluginLoader = new Mock<IPluginLoader>();
		}

		[Test]
		public void TestLoadDifferentPluginTypes()
		{
			var cache = new PluginCache(_pluginLoader.Object);
			cache.LoadAllOfType<IWidgetPlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<IWidgetPlugin>(), Times.Once);
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogAnalyserPlugin>(), Times.Never);

			cache.LoadAllOfType<ILogAnalyserPlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<IWidgetPlugin>(), Times.Once);
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogAnalyserPlugin>(), Times.Once);
		}

		[Test]
		public void TestLoadPluginOnlyOnce()
		{
			var cache = new PluginCache(_pluginLoader.Object);
			cache.LoadAllOfType<IWidgetPlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<IWidgetPlugin>(), Times.Once);
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogAnalyserPlugin>(), Times.Never);

			cache.LoadAllOfType<IWidgetPlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<IWidgetPlugin>(), Times.Once,
			                     "because the cache should not have queried the plugin loader again");
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogAnalyserPlugin>(), Times.Never);
		}
	}
}
