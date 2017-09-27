using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.QuickInfo;

namespace Tailviewer.Test.Ui.Controls.Analyse.Widgets.QuickInfo
{
	[TestFixture]
	public sealed class QuickInfoViewConfigurationTest
	{
		[Test]
		public void TestCtor()
		{
			var config = new QuickInfoViewConfiguration();
			config.Name.Should().Be("New Quick Info");
			config.Format.Should().Be("{message}");
		}
	}
}