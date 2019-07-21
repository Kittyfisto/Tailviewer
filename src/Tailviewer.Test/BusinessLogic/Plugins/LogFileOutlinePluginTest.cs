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
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(ILogFileOutlinePlugin)).Should().Be(new PluginInterfaceVersion(1), "because this interface hasn't been changed yet");
		}
	}
}