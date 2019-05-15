using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
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
		public void TestConstruction()
		{
			var settings = new MainWindowSettings();
			settings.AlwaysOnTop.Should().BeFalse();
		}

		[Test]
		[RequiresThread(ApartmentState.STA)]
		public void TestRestoreTo1([Values(true, false)] bool alwaysOnTop)
		{
			var settings = new MainWindowSettings
			{
				Width = 9001,
				Height = 42,
				AlwaysOnTop = alwaysOnTop
			};
			var window = new Window();
			settings.RestoreTo(window);
			window.Width.Should().Be(9001);
			window.Height.Should().Be(42);
			window.Topmost.Should().Be(alwaysOnTop);
		}

		[Test]
		public void TestClone([Values(true, false)] bool alwaysOnTop)
		{
			var settings = new MainWindowSettings();
			settings.SelectedMainPanel = "Bar";
			settings.SelectedSidePanel = "Foo";
			settings.Height = 10;
			settings.Width = 100;
			settings.Left = 42;
			settings.Top = 101;
			settings.AlwaysOnTop = alwaysOnTop;

			var cloned = settings.Clone();
			cloned.Should().NotBeNull();
			cloned.Should().NotBeSameAs(settings);
			cloned.SelectedMainPanel.Should().Be("Bar");
			cloned.SelectedSidePanel.Should().Be("Foo");
			cloned.Height.Should().Be(10);
			cloned.Width.Should().Be(100);
			cloned.Left.Should().Be(42);
			cloned.Top.Should().Be(101);
			cloned.AlwaysOnTop.Should().Be(alwaysOnTop);
		}

		[Test]
		public void TestSaveRestore([Values(true, false)]bool alwaysOnTop)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");

					var settings = new MainWindowSettings();
					settings.SelectedMainPanel = "Bar";
					settings.SelectedSidePanel = "Foo";
					settings.Height = 10;
					settings.Width = 100;
					settings.Left = 42;
					settings.Top = 101;
					settings.AlwaysOnTop = alwaysOnTop;
					settings.Save(writer);
				}
				stream.Position = 0;
				//Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new MainWindowSettings();
					settings.Restore(reader);
					settings.SelectedMainPanel.Should().Be("Bar");
					settings.SelectedSidePanel.Should().Be("Foo");
					settings.Height.Should().Be(10);
					settings.Width.Should().Be(100);
					settings.Left.Should().Be(42);
					settings.Top.Should().Be(101);
					settings.AlwaysOnTop.Should().Be(alwaysOnTop);
				}
			}
		}
	}
}