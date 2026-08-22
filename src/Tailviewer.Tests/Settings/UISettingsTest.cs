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
	public sealed class UISettingsTest
	{
		private static UISettings Restore(string xml)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			using (var reader = XmlReader.Create(stream))
			{
				reader.MoveToContent();

				var settings = new UISettings();
				settings.Restore(reader);
				return settings;
			}
		}

		private static string Save(UISettings settings)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");
					settings.Save(writer);
					writer.WriteEndElement();
				}

				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		[Test]
		public void TestConstruction()
		{
			var settings = new UISettings();
			settings.Language.Should().Be(UISettings.DefaultLanguage);
			settings.ThemeColor.Should().Be(UISettings.DefaultThemeColor);
		}

		[Test]
		public void TestDefaultThemeColorIs0047AB()
		{
			UISettings.DefaultThemeColor.Should().Be(Color.FromRgb(0x00, 0x47, 0xAB));
		}

		[Test]
		public void TestClone()
		{
			var settings = new UISettings
			{
				Language = "zh-CN",
				ThemeColor = Colors.Red
			};

			var clone = settings.Clone();
			clone.Should().NotBeSameAs(settings);
			clone.Language.Should().Be("zh-CN");
			clone.ThemeColor.Should().Be(Colors.Red);
		}

		[Test]
		public void TestRoundtrip()
		{
			var settings = new UISettings
			{
				Language = "zh-CN",
				ThemeColor = Color.FromRgb(0x12, 0x34, 0x56)
			};

			var restored = Restore(Save(settings));
			restored.Language.Should().Be("zh-CN");
			restored.ThemeColor.Should().Be(Color.FromRgb(0x12, 0x34, 0x56));
		}

		[Test]
		public void TestRestoreFromEmpty()
		{
			var restored = Restore("<Test />");
			restored.Language.Should().Be(UISettings.DefaultLanguage);
			restored.ThemeColor.Should().Be(UISettings.DefaultThemeColor);
		}

		[Test]
		public void TestRestoreFromInvalidColor()
		{
			var restored = Restore("<Test themecolor=\"banana\" />");
			restored.ThemeColor.Should().Be(UISettings.DefaultThemeColor);
		}
	}
}
