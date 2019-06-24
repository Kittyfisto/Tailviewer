using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class WidgetPluginTest
	{
		[Test]
		public void TestPluginVersion()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(IWidgetPlugin)).Should().Be(new PluginInterfaceVersion(2),
			                                                                                       "because the widget plugin been broken once");
		}
	}
}
