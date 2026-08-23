using System.Threading;
using System.Windows.Media;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
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

		[Test]
		public void TestUpdateNeutral()
		{
			TextBrushes.UpdateNeutral(true);

			TextBrushes.CanvasBackgroundBrush.Color.Should().Be(Color.FromRgb(0x1E, 0x1E, 0x1E));
			TextBrushes.DefaultForegroundBrush.Color.Should().Be(Color.FromRgb(0xDC, 0xDC, 0xDC));
			TextBrushes.SelectedUnfocusedBackgroundBrush.Color.Should().Be(Color.FromRgb(0x3F, 0x3F, 0x46));
			TextBrushes.AlternatingBackgroundBrush.Color.Should().Be(Color.FromRgb(0x25, 0x25, 0x26));
			TextBrushes.DataSourceFilenameForegroundBrush.Color.Should().Be(Color.FromRgb(0x9E, 0x9E, 0x9E));

			TextBrushes.UpdateNeutral(false);

			TextBrushes.CanvasBackgroundBrush.Color.Should().Be(Colors.White);
			TextBrushes.DefaultForegroundBrush.Color.Should().Be(Colors.Black);
			TextBrushes.SelectedUnfocusedBackgroundBrush.Color.Should().Be(Color.FromRgb(0xD7, 0xD7, 0xD7));
			TextBrushes.AlternatingBackgroundBrush.Color.Should().Be(Color.FromRgb(0xE8, 0xF1, 0xF7));
			TextBrushes.DataSourceFilenameForegroundBrush.Color.Should().Be(Color.FromRgb(0x80, 0x80, 0x80));
		}

		[Test]
		public void TestDefaultBrushesAreFrozen()
		{
			TextBrushes.CanvasBackgroundBrush.IsFrozen.Should().BeTrue();
			TextBrushes.DefaultForegroundBrush.IsFrozen.Should().BeTrue();
			TextBrushes.SelectedBackgroundBrush.IsFrozen.Should().BeTrue();

			var brushes = new TextBrushes(new LogViewerSettings(), LogLevelPalette.Dark);
			brushes.ForegroundBrush(false, false, true, LevelFlags.Warning).IsFrozen.Should().BeTrue();
			brushes.BackgroundBrush(false, false, true, LevelFlags.Warning, 0).IsFrozen.Should().BeTrue();
		}

		[Test]
		public void TestSelectedUnfocusedForegroundUsesDefaultBrush()
		{
			TextBrushes.UpdateNeutral(false);
			var brushes = new TextBrushes(new LogViewerSettings());

			var light = (SolidColorBrush) brushes.ForegroundBrush(true, false, false, LevelFlags.Info);
			light.Should().BeSameAs(TextBrushes.DefaultForegroundBrush);
			light.Color.Should().Be(Colors.Black);

			TextBrushes.UpdateNeutral(true);
			var dark = (SolidColorBrush) brushes.ForegroundBrush(true, false, false, LevelFlags.Info);
			dark.Should().BeSameAs(TextBrushes.DefaultForegroundBrush);
			dark.Color.Should().Be(Color.FromRgb(0xDC, 0xDC, 0xDC));

			var info = (SolidColorBrush) brushes.ForegroundBrush(true, false, true, LevelFlags.Info);
			info.Should().NotBeSameAs(TextBrushes.DefaultForegroundBrush);
			info.Color.Should().Be(LogViewerSettings.DefaultInfo.ForegroundColor);

			TextBrushes.UpdateNeutral(false);
		}

		[Test]
		public void TestAlternateBackgroundFollowsNeutralTheme()
		{
			var settings = new LogViewerSettings();
			settings.Warning.IsCustom = true;
			settings.Warning.BackgroundColor = Color.FromRgb(0x10, 0x20, 0x30);

			TextBrushes.UpdateNeutral(false);
			var brushesLight = new TextBrushes(settings);

			var defaultAlternating = brushesLight.BackgroundBrush(false, false, true, LevelFlags.Info, 1);
			defaultAlternating.Should().BeSameAs(TextBrushes.AlternatingBackgroundBrush);
			((SolidColorBrush) defaultAlternating).Color.Should().Be(Color.FromRgb(0xE8, 0xF1, 0xF7));

			var userAlternating = brushesLight.BackgroundBrush(false, false, true, LevelFlags.Warning, 1);

			TextBrushes.UpdateNeutral(true);
			var brushesDark = new TextBrushes(settings);

			var defaultAlternatingDark = brushesDark.BackgroundBrush(false, false, true, LevelFlags.Info, 1);
			defaultAlternatingDark.Should().BeSameAs(TextBrushes.AlternatingBackgroundBrush);
			((SolidColorBrush) defaultAlternatingDark).Color.Should().Be(Color.FromRgb(0x25, 0x25, 0x26));

			((SolidColorBrush) userAlternating).Color.Should().Be(Color.FromRgb(0x10, 0x20, 0x30));

			TextBrushes.UpdateNeutral(false);
		}

		[Test]
		public void TestNonCustomLevelBrushUsesPalette()
		{
			var lightBrushes = new TextBrushes(new LogViewerSettings(), LogLevelPalette.Light);
			((SolidColorBrush) lightBrushes.ForegroundBrush(false, false, true, LevelFlags.Info)).Color
				.Should().Be(LogLevelDefaults.Light.Info.ForegroundColor);

			var darkBrushes = new TextBrushes(new LogViewerSettings(), LogLevelPalette.Dark);
			((SolidColorBrush) darkBrushes.ForegroundBrush(false, false, true, LevelFlags.Info)).Color
				.Should().Be(LogLevelDefaults.Dark.Info.ForegroundColor);

			var defaultBrushes = new TextBrushes(new LogViewerSettings());
			((SolidColorBrush) defaultBrushes.ForegroundBrush(false, false, true, LevelFlags.Info)).Color
				.Should().Be(LogLevelDefaults.Light.Info.ForegroundColor);
		}

		[Test]
		public void TestCustomLevelBrushIsPreservedAcrossPalette()
		{
			var settings = new LogViewerSettings();
			settings.Info.IsCustom = true;
			settings.Info.ForegroundColor = Color.FromRgb(0x11, 0x22, 0x33);
			settings.Info.BackgroundColor = Color.FromRgb(0x44, 0x55, 0x66);

			var lightBrushes = new TextBrushes(settings, LogLevelPalette.Light);
			((SolidColorBrush) lightBrushes.ForegroundBrush(false, false, true, LevelFlags.Info)).Color
				.Should().Be(Color.FromRgb(0x11, 0x22, 0x33));

			var darkBrushes = new TextBrushes(settings, LogLevelPalette.Dark);
			((SolidColorBrush) darkBrushes.ForegroundBrush(false, false, true, LevelFlags.Info)).Color
				.Should().Be(Color.FromRgb(0x11, 0x22, 0x33));
		}
	}
}
