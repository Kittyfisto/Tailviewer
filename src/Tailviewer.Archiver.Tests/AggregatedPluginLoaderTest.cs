using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Tests
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
