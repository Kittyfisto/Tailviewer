using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Plugins;

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
			cache.LoadAllOfType<ILogFileOutlinePlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogFileOutlinePlugin>(), Times.Once);
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogEntryParserPlugin>(), Times.Never);

			cache.LoadAllOfType<ILogEntryParserPlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogFileOutlinePlugin>(), Times.Once);
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogEntryParserPlugin>(), Times.Once);
		}

		[Test]
		public void TestLoadPluginOnlyOnce()
		{
			var cache = new PluginCache(_pluginLoader.Object);
			cache.LoadAllOfType<ILogFileOutlinePlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogFileOutlinePlugin>(), Times.Once);
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogEntryParserPlugin>(), Times.Never);

			cache.LoadAllOfType<ILogFileOutlinePlugin>();
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogFileOutlinePlugin>(), Times.Once,
			                     "because the cache should not have queried the plugin loader again");
			_pluginLoader.Verify(x => x.LoadAllOfTypeWithDescription<ILogEntryParserPlugin>(), Times.Never);
		}
	}
}
