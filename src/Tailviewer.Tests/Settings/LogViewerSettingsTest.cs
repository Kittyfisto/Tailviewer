using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Tests.Settings
{
	[TestFixture]
	public sealed class LogViewerSettingsTest
	{
		[Pure]
		private static LogViewerSettings Restore(string file)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(file)))
			{
				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new LogViewerSettings();
					settings.Restore(reader);
					return settings;
				}
			}
		}

		[Pure]
		private static string Save(LogViewerSettings logViewerSettings)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("Test");
					logViewerSettings.Save(writer);
					writer.WriteEndElement();
					writer.WriteEndDocument();
				}

				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		[Test]
		[Description("Verifies that all properties are initialized to sensible default values")]
		public void TestConstruction()
		{
			var settings = new LogViewerSettings();
			settings.LinesScrolledPerWheelTick.Should().Be(2);
			settings.FontSize.Should().Be(12);
			settings.TabWidth.Should().Be(4);

			AssertMatches(settings.Other, LogLevelDefaults.Light.Other);
			AssertMatches(settings.Trace, LogLevelDefaults.Light.Trace);
			AssertMatches(settings.Debug, LogLevelDefaults.Light.Debug);
			AssertMatches(settings.Info, LogLevelDefaults.Light.Info);
			AssertMatches(settings.Warning, LogLevelDefaults.Light.Warning);
			AssertMatches(settings.Error, LogLevelDefaults.Light.Error);
			AssertMatches(settings.Fatal, LogLevelDefaults.Light.Fatal);
		}

		[Test]
		public void TestClone([Values(1, 2)] int linesScrolledPerWheelTick,
		                      [Values(1, 2)] int fontSize,
		                      [Values(1, 2)] int tabWidth)
		{
			var settings = new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesScrolledPerWheelTick,
				FontSize = fontSize,
				TabWidth = tabWidth,
				Other = { BackgroundColor = Colors.Red, ForegroundColor = Colors.Firebrick },
				Trace = { BackgroundColor = Colors.Magenta, ForegroundColor = Colors.Teal },
				Debug = { BackgroundColor = Colors.Aqua, ForegroundColor = Colors.DodgerBlue },
				Info = { BackgroundColor = Colors.DarkOrange, ForegroundColor = Colors.AliceBlue },
				Warning = { BackgroundColor = Colors.Aquamarine, ForegroundColor = Colors.BlanchedAlmond },
				Error = { BackgroundColor = Colors.BlueViolet, ForegroundColor = Colors.CadetBlue },
				Fatal = { BackgroundColor = Colors.Coral, ForegroundColor = Colors.HotPink }
			};

			var actualSettings = settings.Clone();
			actualSettings.LinesScrolledPerWheelTick.Should().Be(linesScrolledPerWheelTick);
			actualSettings.FontSize.Should().Be(fontSize);
			actualSettings.TabWidth.Should().Be(tabWidth);
			actualSettings.Other.BackgroundColor.Should().Be(Colors.Red);
			actualSettings.Other.ForegroundColor.Should().Be(Colors.Firebrick);
			actualSettings.Trace.BackgroundColor.Should().Be(Colors.Magenta);
			actualSettings.Trace.ForegroundColor.Should().Be(Colors.Teal);
			actualSettings.Debug.BackgroundColor.Should().Be(Colors.Aqua);
			actualSettings.Debug.ForegroundColor.Should().Be(Colors.DodgerBlue);
			actualSettings.Info.BackgroundColor.Should().Be(Colors.DarkOrange);
			actualSettings.Info.ForegroundColor.Should().Be(Colors.AliceBlue);
			actualSettings.Warning.BackgroundColor.Should().Be(Colors.Aquamarine);
			actualSettings.Warning.ForegroundColor.Should().Be(Colors.BlanchedAlmond);
			actualSettings.Error.BackgroundColor.Should().Be(Colors.BlueViolet);
			actualSettings.Error.ForegroundColor.Should().Be(Colors.CadetBlue);
			actualSettings.Fatal.BackgroundColor.Should().Be(Colors.Coral);
			actualSettings.Fatal.ForegroundColor.Should().Be(Colors.HotPink);
		}

		[Test]
		public void TestRestoreFromEmpty()
		{
			string empty;
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("Test");
					writer.WriteEndElement();
					writer.WriteEndDocument();
				}

				empty = Encoding.UTF8.GetString(stream.ToArray());
			}

			var settings = Restore(empty);
			const string reason =
				"because when the attribute doesn't appear in the xml content, then its default value shall be restored";
			settings.LinesScrolledPerWheelTick.Should().Be(2, reason);
			settings.FontSize.Should().Be(12, reason);
			settings.TabWidth.Should().Be(4, reason);

			AssertMatches(settings.Other, LogLevelDefaults.Light.Other);
			AssertMatches(settings.Trace, LogLevelDefaults.Light.Trace);
			AssertMatches(settings.Debug, LogLevelDefaults.Light.Debug);
			AssertMatches(settings.Info, LogLevelDefaults.Light.Info);
			AssertMatches(settings.Warning, LogLevelDefaults.Light.Warning);
			AssertMatches(settings.Error, LogLevelDefaults.Light.Error);
			AssertMatches(settings.Fatal, LogLevelDefaults.Light.Fatal);
		}

		[Test]
		[Description("Legacy files without iscustom that hold non-default colors are treated as custom")]
		public void TestRestoreLegacyCustomLevel()
		{
			var settings = Restore("<Test><warning foregroundcolor=\"#FF112233\" backgroundcolor=\"#FF445566\" /></Test>");
			settings.Warning.IsCustom.Should().BeTrue();
			settings.Warning.ForegroundColor.Should().Be(Color.FromRgb(0x11, 0x22, 0x33));
			settings.Warning.BackgroundColor.Should().Be(Color.FromRgb(0x44, 0x55, 0x66));
		}

		[Test]
		[Description("Legacy files without iscustom that hold the light default colors stay non-custom")]
		public void TestRestoreLegacyDefaultLevel()
		{
			var lightInfo = LogLevelDefaults.Light.Info;
			var xml = string.Format(
				"<Test><info foregroundcolor=\"{0}\" backgroundcolor=\"{1}\" /></Test>",
				lightInfo.ForegroundColor,
				lightInfo.BackgroundColor);
			var settings = Restore(xml);
			settings.Info.IsCustom.Should().BeFalse();
		}

		[Test]
		[Description("Explicit iscustom flags are honored regardless of the stored colors")]
		public void TestRestoreExplicitCustomFlag()
		{
			var settings = Restore("<Test><info iscustom=\"true\" foregroundcolor=\"#FF333333\" backgroundcolor=\"#00FFFFFF\" /></Test>");
			settings.Info.IsCustom.Should().BeTrue();
		}

		[Test]
		[Description("ApplyThemeDefaults only rewrites non-custom levels")]
		public void TestApplyThemeDefaultsOnlyChangesNonCustomLevels()
		{
			var settings = new LogViewerSettings();
			settings.Warning.IsCustom = true;
			settings.Warning.ForegroundColor = Colors.Pink;
			settings.Warning.BackgroundColor = Colors.Blue;

			settings.ApplyThemeDefaults(darkMode: true);

			settings.Warning.IsCustom.Should().BeTrue();
			settings.Warning.ForegroundColor.Should().Be(Colors.Pink);
			settings.Warning.BackgroundColor.Should().Be(Colors.Blue);

			settings.Info.IsCustom.Should().BeFalse();
			settings.Info.ForegroundColor.Should().Be(LogLevelDefaults.Dark.Info.ForegroundColor);
			settings.Info.BackgroundColor.Should().Be(LogLevelDefaults.Dark.Info.BackgroundColor);
			settings.Fatal.ForegroundColor.Should().Be(LogLevelDefaults.Dark.Fatal.ForegroundColor);
			settings.Fatal.BackgroundColor.Should().Be(LogLevelDefaults.Dark.Fatal.BackgroundColor);
		}

		[Test]
		[Description("Restoring default colors clears every custom level and restores the light baseline")]
		public void TestRestoreDefaultColors()
		{
			var settings = new LogViewerSettings();
			var levels = new[]
			{
				settings.Other,
				settings.Trace,
				settings.Debug,
				settings.Info,
				settings.Warning,
				settings.Error,
				settings.Fatal
			};
			var defaults = new[]
			{
				LogLevelDefaults.Light.Other,
				LogLevelDefaults.Light.Trace,
				LogLevelDefaults.Light.Debug,
				LogLevelDefaults.Light.Info,
				LogLevelDefaults.Light.Warning,
				LogLevelDefaults.Light.Error,
				LogLevelDefaults.Light.Fatal
			};

			foreach (var level in levels)
			{
				level.IsCustom = true;
				level.ForegroundColor = Colors.Pink;
				level.BackgroundColor = Colors.Blue;
			}

			settings.RestoreDefaultColors();

			for (var i = 0; i < levels.Length; ++i)
			{
				levels[i].IsCustom.Should().BeFalse("because restored levels must follow the active theme defaults");
				AssertMatches(levels[i], defaults[i]);
			}
		}

		[Test]
		[Description("Verifies that upon restoration, invalid values are replaced with defaults")]
		public void TestRestoreFromInvalidValues([Values(-5, -2, -1, 0)] int linesScrolledPerWheelTick,
		                                         [Values(-5, -1, 0)] int fontSize,
		                                         [Values(-1, 0)] int tabWidth)
		{
			var file = Save(new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesScrolledPerWheelTick,
				FontSize = fontSize,
				TabWidth = tabWidth,
				Other = { BackgroundColor = Colors.Red, ForegroundColor = Colors.Firebrick },
				Trace = { BackgroundColor = Colors.Magenta, ForegroundColor = Colors.Teal },
				Debug = { BackgroundColor = Colors.Aqua, ForegroundColor = Colors.DodgerBlue },
				Info = { BackgroundColor = Colors.DarkOrange, ForegroundColor = Colors.AliceBlue },
				Warning = { BackgroundColor = Colors.Aquamarine, ForegroundColor = Colors.BlanchedAlmond },
				Error = { BackgroundColor = Colors.BlueViolet, ForegroundColor = Colors.CadetBlue },
				Fatal = { BackgroundColor = Colors.Coral, ForegroundColor = Colors.HotPink }
			});

			var actualSettings = Restore(file);
			actualSettings.LinesScrolledPerWheelTick.Should().Be(2, "because restore should simply discard invalid values and restore them to their defaults");
			actualSettings.FontSize.Should().Be(12);
			actualSettings.TabWidth.Should().Be(4);
			actualSettings.Other.BackgroundColor.Should().Be(Colors.Red);
			actualSettings.Other.ForegroundColor.Should().Be(Colors.Firebrick);
			actualSettings.Trace.BackgroundColor.Should().Be(Colors.Magenta);
			actualSettings.Trace.ForegroundColor.Should().Be(Colors.Teal);
			actualSettings.Debug.BackgroundColor.Should().Be(Colors.Aqua);
			actualSettings.Debug.ForegroundColor.Should().Be(Colors.DodgerBlue);
			actualSettings.Info.BackgroundColor.Should().Be(Colors.DarkOrange);
			actualSettings.Info.ForegroundColor.Should().Be(Colors.AliceBlue);
			actualSettings.Warning.BackgroundColor.Should().Be(Colors.Aquamarine);
			actualSettings.Warning.ForegroundColor.Should().Be(Colors.BlanchedAlmond);
			actualSettings.Error.BackgroundColor.Should().Be(Colors.BlueViolet);
			actualSettings.Error.ForegroundColor.Should().Be(Colors.CadetBlue);
			actualSettings.Fatal.BackgroundColor.Should().Be(Colors.Coral);
			actualSettings.Fatal.ForegroundColor.Should().Be(Colors.HotPink);
		}

		[Test]
		public void TestRoundtrip([Values(1, 2)] int linesScrolledPerWheelTick,
		                          [Values(1, 20)] int fontSize,
		                          [Values(1, 2)] int tabWidth)
		{
			var settings = new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesScrolledPerWheelTick,
				FontSize = fontSize,
				TabWidth = tabWidth,
				Other = { BackgroundColor = Colors.Red, ForegroundColor = Colors.Firebrick },
				Trace = { BackgroundColor = Colors.Magenta, ForegroundColor = Colors.Teal },
				Debug = { BackgroundColor = Colors.Aqua, ForegroundColor = Colors.DodgerBlue },
				Info = { BackgroundColor = Colors.DarkOrange, ForegroundColor = Colors.AliceBlue },
				Warning = { BackgroundColor = Colors.Aquamarine, ForegroundColor = Colors.BlanchedAlmond },
				Error = { BackgroundColor = Colors.BlueViolet, ForegroundColor = Colors.CadetBlue },
				Fatal = { BackgroundColor = Colors.Coral, ForegroundColor = Colors.HotPink }
			};

			var actualSettings = Restore(Save(settings));
			const string reason = "because all values should roundtrip perfectly";
			actualSettings.LinesScrolledPerWheelTick.Should().Be(linesScrolledPerWheelTick, reason);
			actualSettings.FontSize.Should().Be(fontSize, reason);
			actualSettings.TabWidth.Should().Be(tabWidth);
			actualSettings.Other.BackgroundColor.Should().Be(Colors.Red);
			actualSettings.Other.ForegroundColor.Should().Be(Colors.Firebrick);
			actualSettings.Trace.BackgroundColor.Should().Be(Colors.Magenta);
			actualSettings.Trace.ForegroundColor.Should().Be(Colors.Teal);
			actualSettings.Debug.BackgroundColor.Should().Be(Colors.Aqua);
			actualSettings.Debug.ForegroundColor.Should().Be(Colors.DodgerBlue);
			actualSettings.Info.BackgroundColor.Should().Be(Colors.DarkOrange);
			actualSettings.Info.ForegroundColor.Should().Be(Colors.AliceBlue);
			actualSettings.Warning.BackgroundColor.Should().Be(Colors.Aquamarine);
			actualSettings.Warning.ForegroundColor.Should().Be(Colors.BlanchedAlmond);
			actualSettings.Error.BackgroundColor.Should().Be(Colors.BlueViolet);
			actualSettings.Error.ForegroundColor.Should().Be(Colors.CadetBlue);
			actualSettings.Fatal.BackgroundColor.Should().Be(Colors.Coral);
			actualSettings.Fatal.ForegroundColor.Should().Be(Colors.HotPink);
		}

		private static void AssertMatches(LogLevelSettings actual, LogLevelSettings expected)
		{
			actual.ForegroundColor.Should().Be(expected.ForegroundColor);
			actual.BackgroundColor.Should().Be(expected.BackgroundColor);
		}
	}
}
