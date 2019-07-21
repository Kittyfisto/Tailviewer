using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class LogFileFormatMatcherPluginTest
	{
		[Test]
		public void TestFileFormatPluginVersion()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(ILogFileFormatMatcherPlugin)).Should().Be(new PluginInterfaceVersion(1), "because this interface hasn't been modified yet");
		}
	}
}