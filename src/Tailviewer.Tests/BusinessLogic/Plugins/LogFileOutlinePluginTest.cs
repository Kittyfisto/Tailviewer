using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Tests.BusinessLogic.Plugins
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