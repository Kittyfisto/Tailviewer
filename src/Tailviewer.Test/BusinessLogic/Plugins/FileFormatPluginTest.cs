using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class FileFormatPluginTest
	{
		[Test]
		public void TestFileFormatPluginVersion()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(IFileFormatPlugin)).Should().Be(new PluginInterfaceVersion(2), "because this interface has been broken once");
		}
	}
}
