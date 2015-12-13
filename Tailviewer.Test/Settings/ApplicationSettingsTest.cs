using System.IO;
using System.Windows;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class ApplicationSettingsTest
	{
		[Test]
		public void TestStoreRestore()
		{
			var fileName = Path.GetTempFileName();
			var settings = new ApplicationSettings();
			settings.MainWindow.Left = 1;
			settings.MainWindow.Top = 2;
			settings.MainWindow.Width = 3;
			settings.MainWindow.Height = 4;
			settings.MainWindow.State = WindowState.Maximized;
			settings.DataSources.Add(new DataSourceSettings(@"SharpRemote.Host.1600.log")
				{
					IsOpen = true,
					FollowTail = true,
					StringFilter = "foobar",
					LevelFilter = LevelFlags.Debug
				});
			settings.Save(fileName).Should().BeTrue();

			settings = new ApplicationSettings();
			settings.Restore(fileName);
			settings.MainWindow.Left.Should().Be(1);
			settings.MainWindow.Top.Should().Be(2);
			settings.MainWindow.Width.Should().Be(3);
			settings.MainWindow.Height.Should().Be(4);
			settings.MainWindow.State.Should().Be(WindowState.Maximized);
			settings.DataSources.Count.Should().Be(1);
			settings.DataSources[0].File.Should().Be(@"SharpRemote.Host.1600.log");
			settings.DataSources[0].IsOpen.Should().BeTrue();
			settings.DataSources[0].FollowTail.Should().BeTrue();
			settings.DataSources[0].StringFilter.Should().Be("foobar");
			settings.DataSources[0].LevelFilter.Should().Be(LevelFlags.Debug);
		}
	}
}