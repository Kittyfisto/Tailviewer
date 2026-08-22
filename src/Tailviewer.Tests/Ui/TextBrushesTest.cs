using System.Threading;
using System.Windows.Media;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Tests.Ui
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class TextBrushesTest
	{
		[Test]
		public void TestDefaultsFollowThemeColor()
		{
			TextBrushes.SelectedBackgroundBrush.Color.Should().Be(UISettings.DefaultThemeColor);
			TextBrushes.LineNumberForegroundBrush.Color.Should().Be(UISettings.DefaultThemeColor);
			TextBrushes.DataSourceCharacterCodeForegroundBrush.Color.Should().Be(UISettings.DefaultThemeColor);
		}

		[Test]
		public void TestUpdateTheme()
		{
			var primary = Color.FromRgb(0x12, 0x34, 0x56);
			TextBrushes.UpdateTheme(primary);

			TextBrushes.SelectedBackgroundBrush.Color.Should().Be(primary);
			TextBrushes.LineNumberForegroundBrush.Color.Should().Be(primary);
			TextBrushes.DataSourceCharacterCodeForegroundBrush.Color.Should().Be(primary);

			TextBrushes.UpdateTheme(UISettings.DefaultThemeColor);
		}
	}
}
