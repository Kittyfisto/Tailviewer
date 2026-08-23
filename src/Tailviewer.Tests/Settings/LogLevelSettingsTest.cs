using System.Collections.Generic;
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
	public sealed class LogLevelSettingsTest
	{
		public static IReadOnlyList<Color> SomeColors => new[]
		{
			Colors.Black,
			Colors.Magenta,
			Colors.Teal
		};

		[Pure]
		private static LogLevelSettings Restore(string file)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(file)))
			{
				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new LogLevelSettings();
					settings.Restore(reader);
					return settings;
				}
			}
		}

		[Pure]
		private static string Save(LogLevelSettings logViewerSettings)
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
		public void TestConstruction()
		{
			var settings = new LogLevelSettings();
			settings.ForegroundColor.Should().Be(Colors.Black);
			settings.BackgroundColor.Should().Be(Colors.Transparent);
			settings.IsCustom.Should().BeFalse();
		}

		[Test]
		public void TestIsCustomRoundtrip()
		{
			var settings = new LogLevelSettings
			{
				ForegroundColor = Colors.Red,
				BackgroundColor = Colors.Blue,
				IsCustom = true
			};
			var restored = Restore(Save(settings));
			restored.ForegroundColor.Should().Be(Colors.Red);
			restored.BackgroundColor.Should().Be(Colors.Blue);
			restored.IsCustom.Should().BeTrue();
		}

		[Test]
		public void TestIsCustomNotPresentRestoresFalse()
		{
			var restored = Restore("<Test foregroundcolor=\"#FF0000\" backgroundcolor=\"#0000FF\" />");
			restored.ForegroundColor.Should().Be(Colors.Red);
			restored.BackgroundColor.Should().Be(Colors.Blue);
			restored.IsCustom.Should().BeFalse();
		}

		[Test]
		public void TestMatches()
		{
			var settings = new LogLevelSettings
			{
				ForegroundColor = Color.FromRgb(0x01, 0x02, 0x03),
				BackgroundColor = Color.FromRgb(0x04, 0x05, 0x06)
			};

			settings.Matches(Color.FromRgb(0x01, 0x02, 0x03), Color.FromRgb(0x04, 0x05, 0x06)).Should().BeTrue();
			settings.Matches(Color.FromRgb(0x09, 0x09, 0x09), Color.FromRgb(0x04, 0x05, 0x06)).Should().BeFalse();
			settings.Matches(Color.FromRgb(0x01, 0x02, 0x03), Color.FromRgb(0x09, 0x09, 0x09)).Should().BeFalse();
		}

		[Test]
		public void TestRoundtrip([ValueSource(nameof(SomeColors))] Color foregroundColor,
		                          [ValueSource(nameof(SomeColors))] Color backgroundColor)
		{
			var settings = new LogLevelSettings
			{
				ForegroundColor = foregroundColor,
				BackgroundColor = backgroundColor
			};
			var actualSettings = Restore(Save(settings));
			actualSettings.ForegroundColor.Should().Be(foregroundColor);
			actualSettings.BackgroundColor.Should().Be(backgroundColor);
		}
	}
}