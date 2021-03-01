using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class AggregatedPluginLoaderTest
	{
		[Test]
		public void TestLoadPluginEmpty()
		{
			var loader = new AggregatedPluginLoader();
			loader.LoadAllOfType<ILogFileOutlinePlugin>().Should().BeEmpty();
		}
	}
}
