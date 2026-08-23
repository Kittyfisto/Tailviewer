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
			settings.ThemeMode.Should().Be(ThemeMode.Light);
		}

		[Test]
		public void TestDefaultThemeColorIsFF222280()
		{
			UISettings.DefaultThemeColor.Should().Be(Color.FromArgb(0xFF, 0x22, 0x22, 0x80));
		}

		[Test]
		public void TestClone()
		{
			var settings = new UISettings
			{
				Language = "zh-CN",
				ThemeColor = Colors.Red,
				ThemeMode = ThemeMode.System
			};

			var clone = settings.Clone();
			clone.Should().NotBeSameAs(settings);
			clone.Language.Should().Be("zh-CN");
			clone.ThemeColor.Should().Be(Colors.Red);
			clone.ThemeMode.Should().Be(ThemeMode.System);
		}

		[Test]
		public void TestRoundtrip()
		{
			var settings = new UISettings
			{
				Language = "zh-CN",
				ThemeColor = Color.FromRgb(0x12, 0x34, 0x56),
				ThemeMode = ThemeMode.Dark
			};

			var restored = Restore(Save(settings));
			restored.Language.Should().Be("zh-CN");
			restored.ThemeColor.Should().Be(Color.FromRgb(0x12, 0x34, 0x56));
			restored.ThemeMode.Should().Be(ThemeMode.Dark);
		}

		[Test]
		public void TestRestoreFromEmpty()
		{
			var restored = Restore("<Test />");
			restored.Language.Should().Be(UISettings.DefaultLanguage);
			restored.ThemeColor.Should().Be(UISettings.DefaultThemeColor);
			restored.ThemeMode.Should().Be(ThemeMode.Light);
		}

		[Test]
		public void TestRestoreFromInvalidColor()
		{
			var restored = Restore("<Test themecolor=\"banana\" />");
			restored.ThemeColor.Should().Be(UISettings.DefaultThemeColor);
		}

		[Test]
		public void TestRestoreThemeMode()
		{
			Restore("<Test thememode=\"Dark\" />").ThemeMode.Should().Be(ThemeMode.Dark);
			Restore("<Test thememode=\"System\" />").ThemeMode.Should().Be(ThemeMode.System);
			Restore("<Test thememode=\"light\" />").ThemeMode.Should().Be(ThemeMode.Light);
		}

		[Test]
		public void TestRestoreFromInvalidThemeMode()
		{
			Restore("<Test thememode=\"banana\" />").ThemeMode.Should().Be(ThemeMode.Light);
			Restore("<Test thememode=\"99\" />").ThemeMode.Should().Be(ThemeMode.Light);
		}

		[Test]
		public void TestRestoreInvalidThemeModeDoesNotFallBackToLegacyDarkMode()
		{
			Restore("<Test thememode=\"99\" darkmode=\"true\" />").ThemeMode.Should().Be(ThemeMode.Light);
			Restore("<Test thememode=\"banana\" darkmode=\"true\" />").ThemeMode.Should().Be(ThemeMode.Light);
		}

	}
}
