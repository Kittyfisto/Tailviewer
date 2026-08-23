using System.Windows.Media;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Settings;

namespace Tailviewer.Tests.Settings
{
	[TestFixture]
	public sealed class LogLevelDefaultsTest
	{
		[Test]
		public void TestLightPalette()
		{
			var light = LogLevelDefaults.Light;
			AssertLevel(light.Other, Color.FromRgb(0x7C, 0x6E, 0x9E), Colors.Transparent);
			AssertLevel(light.Trace, Color.FromRgb(0x9A, 0xA5, 0xB1), Colors.Transparent);
			AssertLevel(light.Debug, Color.FromRgb(0x6B, 0x7A, 0x8F), Colors.Transparent);
			AssertLevel(light.Info, Color.FromRgb(0x33, 0x33, 0x33), Colors.Transparent);
			AssertLevel(light.Warning, Color.FromRgb(0x8A, 0x4D, 0x00), Color.FromRgb(0xFF, 0xF3, 0xD6));
			AssertLevel(light.Error, Color.FromRgb(0xB3, 0x26, 0x1E), Color.FromRgb(0xFD, 0xEC, 0xEA));
			AssertLevel(light.Fatal, Color.FromRgb(0xFF, 0xFF, 0xFF), Color.FromRgb(0xC5, 0x22, 0x1F));
		}

		[Test]
		public void TestDarkPalette()
		{
			var dark = LogLevelDefaults.Dark;
			AssertLevel(dark.Other, Color.FromRgb(0x8E, 0x7F, 0xAE), Colors.Transparent);
			AssertLevel(dark.Trace, Color.FromRgb(0x5B, 0x64, 0x72), Colors.Transparent);
			AssertLevel(dark.Debug, Color.FromRgb(0x7E, 0x8C, 0x9E), Colors.Transparent);
			AssertLevel(dark.Info, Color.FromRgb(0xDC, 0xDC, 0xDC), Colors.Transparent);
			AssertLevel(dark.Warning, Color.FromRgb(0xF5, 0xD7, 0x7E), Color.FromRgb(0x5A, 0x47, 0x00));
			AssertLevel(dark.Error, Color.FromRgb(0xFF, 0xB4, 0xAB), Color.FromRgb(0x6B, 0x1F, 0x1F));
			AssertLevel(dark.Fatal, Color.FromRgb(0xFF, 0xFF, 0xFF), Color.FromRgb(0x9E, 0x1B, 0x1B));
		}

		[Test]
		public void TestFor()
		{
			LogLevelDefaults.For(false).Should().BeSameAs(LogLevelDefaults.Light);
			LogLevelDefaults.For(true).Should().BeSameAs(LogLevelDefaults.Dark);
		}

		[Test]
		public void TestGet()
		{
			LogLevelDefaults.Light.Get(LevelFlags.Other).Should().BeSameAs(LogLevelDefaults.Light.Other);
			LogLevelDefaults.Light.Get(LevelFlags.Trace).Should().BeSameAs(LogLevelDefaults.Light.Trace);
			LogLevelDefaults.Light.Get(LevelFlags.Debug).Should().BeSameAs(LogLevelDefaults.Light.Debug);
			LogLevelDefaults.Light.Get(LevelFlags.Info).Should().BeSameAs(LogLevelDefaults.Light.Info);
			LogLevelDefaults.Light.Get(LevelFlags.Warning).Should().BeSameAs(LogLevelDefaults.Light.Warning);
			LogLevelDefaults.Light.Get(LevelFlags.Error).Should().BeSameAs(LogLevelDefaults.Light.Error);
			LogLevelDefaults.Light.Get(LevelFlags.Fatal).Should().BeSameAs(LogLevelDefaults.Light.Fatal);
			LogLevelDefaults.Light.Get(LevelFlags.All).Should().BeSameAs(LogLevelDefaults.Light.Other);
			LogLevelDefaults.Light.Get(LevelFlags.None).Should().BeSameAs(LogLevelDefaults.Light.Other);
		}

		private static void AssertLevel(LogLevelSettings level, Color foreground, Color background)
		{
			level.ForegroundColor.Should().Be(foreground);
			level.BackgroundColor.Should().Be(background);
		}
	}
}