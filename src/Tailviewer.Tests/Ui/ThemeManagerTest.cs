using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui;

namespace Tailviewer.Tests.Ui
{
	[TestFixture]
	public sealed class ThemeManagerTest
	{
		private sealed class FakeSystemThemeProvider
			: ISystemThemeProvider
		{
			private readonly bool _isDark;

			public FakeSystemThemeProvider(bool isDark)
			{
				_isDark = isDark;
			}

			public bool IsDark()
			{
				return _isDark;
			}

			#pragma warning disable 67
			public event Action Changed;
			#pragma warning restore 67
		}

		[Test]
		public void TestResolveDarkModeLight()
		{
			ThemeManager.ResolveDarkMode(ThemeMode.Light).Should().BeFalse();
		}

		[Test]
		public void TestResolveDarkModeDark()
		{
			ThemeManager.ResolveDarkMode(ThemeMode.Dark).Should().BeTrue();
		}

		[Test]
		public void TestResolveDarkModeSystemFollowsProvider()
		{
			var previous = ThemeManager.SystemThemeProvider;
			try
			{
				ThemeManager.SystemThemeProvider = new FakeSystemThemeProvider(true);
				ThemeManager.ResolveDarkMode(ThemeMode.System).Should().BeTrue();

				ThemeManager.SystemThemeProvider = new FakeSystemThemeProvider(false);
				ThemeManager.ResolveDarkMode(ThemeMode.System).Should().BeFalse();
			}
			finally
			{
				ThemeManager.SystemThemeProvider = previous;
			}
		}

		[Test]
		public void TestResolveDarkModeSystemWithoutProviderIsLight()
		{
			var previous = ThemeManager.SystemThemeProvider;
			try
			{
				ThemeManager.SystemThemeProvider = null;
				ThemeManager.ResolveDarkMode(ThemeMode.System).Should().BeFalse();
			}
			finally
			{
				ThemeManager.SystemThemeProvider = previous;
			}
		}

		[Test]
		public void TestResolveDarkModeUndefinedValueFallsBackToLight()
		{
			ThemeManager.ResolveDarkMode((ThemeMode)99).Should().BeFalse();
		}

		[Test]
		public void TestApplyThemeModeSelectsMatchingPalette()
		{
			try
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
				ThemeManager.CurrentDarkMode.Should().BeFalse();
				ThemeManager.CurrentThemeMode.Should().Be(ThemeMode.Light);
				ThemeManager.CurrentLogLevelPalette.Should().Be(LogLevelPalette.Light);

				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Dark);
				ThemeManager.CurrentDarkMode.Should().BeTrue();
				ThemeManager.CurrentThemeMode.Should().Be(ThemeMode.Dark);
				ThemeManager.CurrentLogLevelPalette.Should().Be(LogLevelPalette.Dark);
			}
			finally
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}

		[Test]
		public void TestApplySystemThemeSelectsResolvedPalette()
		{
			var previous = ThemeManager.SystemThemeProvider;
			try
			{
				ThemeManager.SystemThemeProvider = new FakeSystemThemeProvider(true);
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.System);
				ThemeManager.CurrentDarkMode.Should().BeTrue();
				ThemeManager.CurrentLogLevelPalette.Should().Be(LogLevelPalette.Dark);

				ThemeManager.SystemThemeProvider = new FakeSystemThemeProvider(false);
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.System);
				ThemeManager.CurrentDarkMode.Should().BeFalse();
				ThemeManager.CurrentLogLevelPalette.Should().Be(LogLevelPalette.Light);
			}
			finally
			{
				ThemeManager.SystemThemeProvider = previous;
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}

		[Test]
		public void TestApplyBoolOverloadSelectsMatchingPalette()
		{
			try
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, true);
				ThemeManager.CurrentDarkMode.Should().BeTrue();
				ThemeManager.CurrentLogLevelPalette.Should().Be(LogLevelPalette.Dark);

				ThemeManager.Apply(UISettings.DefaultThemeColor, false);
				ThemeManager.CurrentDarkMode.Should().BeFalse();
				ThemeManager.CurrentLogLevelPalette.Should().Be(LogLevelPalette.Light);
			}
			finally
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}
	}
}
