using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class LogFileIssuesPluginTest
	{
		[Test]
		public void TestFileFormatPluginVersion()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(ILogFileIssuesPlugin)).Should().Be(new PluginInterfaceVersion(3), "because this interface has been broken twice");
		}
	}
}