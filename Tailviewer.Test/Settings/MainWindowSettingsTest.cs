using System;
using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class MainWindowSettingsTest
	{
		[Test]
		public void TestClone()
		{
			var settings = new MainWindowSettings();
			settings.SelectedSidePanel = "Foo";
			settings.Window.Height = 10;
			settings.Window.Width = 100;
			settings.Window.Left = 42;
			settings.Window.Top = 101;

			var cloned = settings.Clone();
			cloned.Should().NotBeNull();
			cloned.Should().NotBeSameAs(settings);
			cloned.SelectedSidePanel.Should().Be("Foo");
			cloned.Window.Height.Should().Be(10);
			cloned.Window.Width.Should().Be(100);
			cloned.Window.Left.Should().Be(42);
			cloned.Window.Top.Should().Be(101);
		}

		[Test]
		public void TestRoundtrip()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");

					var settings = new MainWindowSettings();
					settings.SelectedSidePanel = "Foo";
					settings.Window.Height = 10;
					settings.Window.Width = 100;
					settings.Window.Left = 42;
					settings.Window.Top = 101;
					settings.Save(writer);
				}
				stream.Position = 0;
				Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new MainWindowSettings();
					settings.Restore(reader);
					settings.SelectedSidePanel.Should().Be("Foo");
					settings.Window.Height.Should().Be(10);
					settings.Window.Width.Should().Be(100);
					settings.Window.Left.Should().Be(42);
					settings.Window.Top.Should().Be(101);
				}
			}
		}
	}
}