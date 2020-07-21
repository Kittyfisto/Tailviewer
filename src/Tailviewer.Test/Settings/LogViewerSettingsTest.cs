using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
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

			settings.Other.ForegroundColor.Should().Be(Colors.Black);
			settings.Other.BackgroundColor.Should().Be(Colors.Transparent);

			settings.Trace.ForegroundColor.Should().Be(Color.FromRgb(128, 128, 128));
			settings.Trace.BackgroundColor.Should().Be(Colors.Transparent);

			settings.Debug.ForegroundColor.Should().Be(Color.FromRgb(128, 128, 128));
			settings.Debug.BackgroundColor.Should().Be(Colors.Transparent);

			settings.Info.ForegroundColor.Should().Be(Colors.Black);
			settings.Info.BackgroundColor.Should().Be(Colors.Transparent);

			settings.Warning.ForegroundColor.Should().Be(Colors.White);
			settings.Warning.BackgroundColor.Should().Be(Color.FromRgb(255, 195, 0));

			settings.Error.ForegroundColor.Should().Be(Colors.White);
			settings.Error.BackgroundColor.Should().Be(Color.FromRgb(232, 17, 35));

			settings.Fatal.ForegroundColor.Should().Be(Colors.White);
			settings.Fatal.BackgroundColor.Should().Be(Color.FromRgb(232, 17, 35));
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

				empty =  Encoding.UTF8.GetString(stream.ToArray());
			}

			var settings = Restore(empty);
			const string reason =
				"because when the attribute doesn't appear in the xml content, then its default value shall be restored";
			settings.LinesScrolledPerWheelTick.Should().Be(2, reason);
			settings.FontSize.Should().Be(12, reason);
			settings.TabWidth.Should().Be(4, reason);

			settings.Other.ForegroundColor.Should().Be(Colors.Black);
			settings.Other.BackgroundColor.Should().Be(Colors.Transparent);

			settings.Trace.ForegroundColor.Should().Be(Color.FromRgb(128, 128, 128), reason);
			settings.Trace.BackgroundColor.Should().Be(Colors.Transparent, reason);

			settings.Debug.ForegroundColor.Should().Be(Color.FromRgb(128, 128, 128));
			settings.Debug.BackgroundColor.Should().Be(Colors.Transparent);

			settings.Info.ForegroundColor.Should().Be(Colors.Black);
			settings.Info.BackgroundColor.Should().Be(Colors.Transparent);

			settings.Warning.ForegroundColor.Should().Be(Colors.White);
			settings.Warning.BackgroundColor.Should().Be(Color.FromRgb(255, 195, 0));

			settings.Error.ForegroundColor.Should().Be(Colors.White);
			settings.Error.BackgroundColor.Should().Be(Color.FromRgb(232, 17, 35));

			settings.Fatal.ForegroundColor.Should().Be(Colors.White);
			settings.Fatal.BackgroundColor.Should().Be(Color.FromRgb(232, 17, 35));
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
	}
}