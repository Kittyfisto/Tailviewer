using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
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
		}

		[Test]
		public void TestClone([Values(1, 2, 3, 4)] int linesScrolledPerWheelTick)
		{
			var settings = new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesScrolledPerWheelTick
			};

			var actualSettings = settings.Clone();
			actualSettings.LinesScrolledPerWheelTick.Should().Be(linesScrolledPerWheelTick);
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
		}

		[Test]
		[Description("Verifies that upon restoration, invalid values are replaced with defaults")]
		public void TestRestoreFromInvalidValues([Values(-5, -2, -1, 0)] int linesScrolledPerWheelTick)
		{
			var file = Save(new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesScrolledPerWheelTick
			});

			var settings = Restore(file);
			settings.LinesScrolledPerWheelTick.Should().Be(2, "because restore should simply discard invalid values and restore them to their defaults");
		}

		[Test]
		public void TestRoundtrip([Values(1, 2, 5)] int linesScrolledPerWheelTick)
		{
			var settings = new LogViewerSettings
			{
				LinesScrolledPerWheelTick = linesScrolledPerWheelTick
			};

			var actualSettings = Restore(Save(settings));
			const string reason = "because all values should roundtrip perfectly";
			actualSettings.LinesScrolledPerWheelTick.Should().Be(linesScrolledPerWheelTick, reason);
		}
	}
}