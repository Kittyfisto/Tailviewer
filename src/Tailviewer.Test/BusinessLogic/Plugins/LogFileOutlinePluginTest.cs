using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Ui.Outline;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class LogFileOutlinePluginTest
	{
		[Test]
		public void TestFileFormatPluginVersion()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(ILogFileOutlinePlugin)).Should().Be(new PluginInterfaceVersion(2), "because this interface has been broken once");
		}
	}
}