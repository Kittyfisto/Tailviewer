using System.IO;
using System.Threading;
using System.Windows;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api.Tests;
using Tailviewer.Settings;

namespace Tailviewer.Tests.Settings
{
	[TestFixture]
	public sealed class MainWindowSettingsTest
	{
		[Test]
		public void TestConstruction()
		{
			var settings = new MainWindowSettings();
			settings.AlwaysOnTop.Should().BeFalse();
			settings.IsLeftSidePanelVisible.Should().BeTrue();
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
		[RequiresThread(ApartmentState.STA)]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/250")]
		public void TestRestoreToMinimized1()
		{
			var settings = new MainWindowSettings
			{
				Width = 9001,
				Height = 42,
				State = WindowState.Normal
			};
			settings.State = WindowState.Minimized;

			var window = new Window();
			settings.RestoreTo(window);
			window.Width.Should().Be(9001);
			window.Height.Should().Be(42);
			window.WindowState.Should().Be(WindowState.Normal);
		}

		[Test]
		[RequiresThread(ApartmentState.STA)]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/250")]
		public void TestRestoreToMinimized2()
		{
			var settings = new MainWindowSettings
			{
				Width = 9001,
				Height = 42,
				State = WindowState.Maximized
			};
			settings.State = WindowState.Minimized;

			var window = new Window();
			settings.RestoreTo(window);
			window.Width.Should().Be(9001);
			window.Height.Should().Be(42);
			window.WindowState.Should().Be(WindowState.Maximized);
		}

		[Test]
		public void TestClone([Values(true, false)] bool alwaysOnTop,
		                      [Values(true, false)] bool isLeftSidePanelCollapsed)
		{
			var settings = new MainWindowSettings();
			settings.SelectedMainPanel = "Bar";
			settings.SelectedSidePanel = "Foo";
			settings.Height = 10;
			settings.Width = 100;
			settings.Left = 42;
			settings.Top = 101;
			settings.AlwaysOnTop = alwaysOnTop;
			settings.IsLeftSidePanelVisible = isLeftSidePanelCollapsed;

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
			cloned.IsLeftSidePanelVisible.Should().Be(isLeftSidePanelCollapsed);
		}

		[Test]
		public void TestSaveRestore([Values(true, false)]bool alwaysOnTop,
		                            [Values(true, false)]bool isLeftSidePanelCollapsed)
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
					settings.IsLeftSidePanelVisible = isLeftSidePanelCollapsed;
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
					settings.IsLeftSidePanelVisible.Should().Be(isLeftSidePanelCollapsed);
				}
			}
		}
	}
}