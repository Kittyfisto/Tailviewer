using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class TextSettingsTest
	{
		[Test]
		public void TestDefault()
		{
			TextSettings.Default.FontSize.Should().Be(12);
			TextSettings.Default.LineSpacing.Should().Be(3);
			TextSettings.Default.LineHeight.Should().Be(15);
			TextSettings.Default.LineNumberSpacing.Should().Be(5);
			TextSettings.Default.Typeface.Should().NotBeNull();
		}
	}
}
