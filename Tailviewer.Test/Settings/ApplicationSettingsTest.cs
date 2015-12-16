using System;
using System.IO;
using System.Linq;
using System.Windows;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using DataSource = Tailviewer.Settings.DataSource;
using QuickFilter = Tailviewer.Settings.QuickFilter;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class ApplicationSettingsTest
	{
		[Test]
		[Description("Verifies that the folder is created if it doesn't exist")]
		public void TestStore1()
		{
			var path = Path.GetTempPath();
			const string folderName = "ApplicationSettingsTest";
			const string fileName = "settings.xml";
			var fileFolder = Path.Combine(path, folderName);
			var filePath = Path.Combine(fileFolder, fileName);

			if (File.Exists(filePath))
				File.Delete(filePath);

			if (Directory.Exists(fileFolder))
				Directory.Delete(fileFolder);

			var settings = new ApplicationSettings(filePath);
			settings.QuickFilters.Add(new QuickFilter());
			settings.Save().Should().BeTrue();

			File.Exists(filePath);
			var settings2 = new ApplicationSettings(filePath);
			settings2.Restore();
			settings2.QuickFilters.Count.Should().Be(1);
		}

		[Test]
		public void TestStoreRestore()
		{
			const string fileName = "applicationsettingstest.xml";
			var settings = new ApplicationSettings(fileName);
			settings.MainWindow.Left = 1;
			settings.MainWindow.Top = 2;
			settings.MainWindow.Width = 3;
			settings.MainWindow.Height = 4;
			settings.MainWindow.State = WindowState.Maximized;
			settings.DataSources.Add(new DataSource(@"SharpRemote.Host.1600.log")
				{
					IsOpen = true,
					FollowTail = true,
					StringFilter = "foobar",
					LevelFilter = LevelFlags.Debug,
					ActivatedQuickFilters =
						{
							Guid.NewGuid(),
							Guid.NewGuid(),
							Guid.NewGuid(),
						}
				});
			var guids = settings.DataSources[0].ActivatedQuickFilters.ToList();
			settings.QuickFilters.Add(new QuickFilter
				{
					Value = "foobar",
					IgnoreCase = true,
					Type = QuickFilterType.RegexpFilter
				});
			var id = settings.QuickFilters[0].Id;
			settings.Save().Should().BeTrue();

			settings = new ApplicationSettings(fileName);
			settings.Restore();
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
			settings.DataSources[0].ActivatedQuickFilters.Should().Equal(guids);

			settings.QuickFilters.Count.Should().Be(1);
			settings.QuickFilters[0].Id.Should().Be(id);
			settings.QuickFilters[0].Value.Should().Be("foobar");
			settings.QuickFilters[0].IgnoreCase.Should().BeTrue();
			settings.QuickFilters[0].Type.Should().Be(QuickFilterType.RegexpFilter);
		}
	}
}