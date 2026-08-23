using System.Threading;
using System.Windows.Media;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Settings;
using Tailviewer.Ui;
using Tailviewer.Ui.Settings;

namespace Tailviewer.Tests.Ui.Settings
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class LogLevelSettingsViewModelTest
	{
		[Test]
		public void TestForegroundOnlyChangeInDarkSeedsDarkDefaults()
		{
			ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Dark);
			try
			{
				var levelSettings = new LogLevelSettings();
				var settings = new Mock<IApplicationSettings>();
				var vm = new LogLevelSettingsViewModel(settings.Object, levelSettings, LevelFlags.Warning);

				vm.ForegroundColor.Should().Be(LogLevelDefaults.Dark.Warning.ForegroundColor);
				vm.BackgroundColor.Should().Be(LogLevelDefaults.Dark.Warning.BackgroundColor);

				var customForeground = Color.FromRgb(0x01, 0x02, 0x03);
				vm.ForegroundColor = customForeground;

				levelSettings.IsCustom.Should().BeTrue();
				levelSettings.ForegroundColor.Should().Be(customForeground);
				levelSettings.BackgroundColor.Should().Be(LogLevelDefaults.Dark.Warning.BackgroundColor);
			}
			finally
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}

		[Test]
		public void TestBackgroundOnlyChangeInDarkSeedsDarkDefaults()
		{
			ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Dark);
			try
			{
				var levelSettings = new LogLevelSettings();
				var settings = new Mock<IApplicationSettings>();
				var vm = new LogLevelSettingsViewModel(settings.Object, levelSettings, LevelFlags.Warning);

				var customBackground = Color.FromRgb(0x04, 0x05, 0x06);
				vm.BackgroundColor = customBackground;

				levelSettings.IsCustom.Should().BeTrue();
				levelSettings.BackgroundColor.Should().Be(customBackground);
				levelSettings.ForegroundColor.Should().Be(LogLevelDefaults.Dark.Warning.ForegroundColor);
			}
			finally
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}

		[Test]
		public void TestForegroundOnlyChangeInLightSeedsLightDefaults()
		{
			ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			try
			{
				var levelSettings = new LogLevelSettings();
				var settings = new Mock<IApplicationSettings>();
				var vm = new LogLevelSettingsViewModel(settings.Object, levelSettings, LevelFlags.Warning);

				var customForeground = Color.FromRgb(0x07, 0x08, 0x09);
				vm.ForegroundColor = customForeground;

				levelSettings.IsCustom.Should().BeTrue();
				levelSettings.ForegroundColor.Should().Be(customForeground);
				levelSettings.BackgroundColor.Should().Be(LogLevelDefaults.Light.Warning.BackgroundColor);
			}
			finally
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}

		[Test]
		public void TestAlreadyCustomSetForegroundPreservesBackground()
		{
			var levelSettings = new LogLevelSettings
			{
				ForegroundColor = Color.FromRgb(0x11, 0x22, 0x33),
				BackgroundColor = Color.FromRgb(0x44, 0x55, 0x66),
				IsCustom = true
			};
			var settings = new Mock<IApplicationSettings>();
			var vm = new LogLevelSettingsViewModel(settings.Object, levelSettings, LevelFlags.Warning);

			var newForeground = Color.FromRgb(0x77, 0x88, 0x99);
			vm.ForegroundColor = newForeground;

			levelSettings.ForegroundColor.Should().Be(newForeground);
			levelSettings.BackgroundColor.Should().Be(Color.FromRgb(0x44, 0x55, 0x66));
		}

		[Test]
		public void TestTransitionCallsSaveAsync()
		{
			ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Dark);
			try
			{
				var levelSettings = new LogLevelSettings();
				var settings = new Mock<IApplicationSettings>();
				var vm = new LogLevelSettingsViewModel(settings.Object, levelSettings, LevelFlags.Warning);

				vm.ForegroundColor = Color.FromRgb(0xAA, 0xBB, 0xCC);

				settings.Verify(x => x.SaveAsync(), Times.Once());
			}
			finally
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}
	}
}
