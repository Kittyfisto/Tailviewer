using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class PluginInterfaceVersionTest
	{
		[Test]
		public void TestEquality()
		{
			new PluginInterfaceVersion(1).Should().Be(new PluginInterfaceVersion(1));
			new PluginInterfaceVersion(1).Should().Be(PluginInterfaceVersion.First);
		}

		[Test]
		public void TestInequality()
		{
			new PluginInterfaceVersion(1).Should().NotBe(new PluginInterfaceVersion(2));
			new PluginInterfaceVersion(2).Should().NotBe(new PluginInterfaceVersion(3));
		}
	}
}
