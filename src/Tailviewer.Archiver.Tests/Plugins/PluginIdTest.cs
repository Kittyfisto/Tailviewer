using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Tests.Plugins
{
	[TestFixture]
	public sealed class PluginIdTest
	{
		[Test]
		public void TestEquality()
		{
			new PluginId("A").Should().Be(new PluginId("A"));
			new PluginId("A").Should().NotBe(new PluginId("a"));
			new PluginId("A").Should().NotBe(new PluginId("â"));
			new PluginId("A").Should().NotBe(new PluginId("b"));
		}

		[Test]
		public void TestToString()
		{
			var id = new PluginId("MyCompany.Namespace.Log");
			id.ToString().Should().Be("MyCompany.Namespace.Log");
		}
	}
}
