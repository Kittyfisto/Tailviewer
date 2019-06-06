using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel.Settings;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class SettingsMainPanelViewModelTest
	{
		[Test]
		public void TestConstruction([Values(true, false)] bool recursive)
		{
			var settings = new ApplicationSettings("foo");
			settings.DataSources.FolderDataSourceRecursive = recursive;
			settings.DataSources.FolderDataSourcePattern = "*.log;*.txt";

			var model = new SettingsMainPanelViewModel(settings);

			model.FolderDataSourceRecursive.Should().Be(recursive);
			model.FolderDataSourcePatterns.Should().Be("*.log;*.txt");
		}

		[Test]
		public void TestSetFolderDataSourcePatterns1([Values(null, "", " ", ";;")] string emptyPattern)
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsMainPanelViewModel(settings);

			model.FolderDataSourcePatterns = emptyPattern;
			settings.DataSources.FolderDataSourcePattern.Should().Be(emptyPattern);
		}

		[Test]
		public void TestSetFolderDataSourcePatternsSkipTrailingSpace()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsMainPanelViewModel(settings);

			model.FolderDataSourcePatterns = "*.log ";
			settings.DataSources.FolderDataSourcePattern.Should().Be("*.log ");
		}

		[Test]
		public void TestSetFolderDataSourceMultiplePatterns()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsMainPanelViewModel(settings);

			model.FolderDataSourcePatterns = "*.txt;*.log ;*log*";
			settings.DataSources.FolderDataSourcePattern.Should().Be("*.txt;*.log ;*log*");
		}

		[Test]
		public void TestSetFolderDataSourcePatternsSkipLeadingSpace()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsMainPanelViewModel(settings);

			model.FolderDataSourcePatterns = " *.log";
			settings.DataSources.FolderDataSourcePattern.Should().Be(" *.log");
		}

		[Test]
		public void TestProxyPassword1()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsMainPanelViewModel(settings);

			var changes = new List<string>();
			model.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);

			model.ProxyPassword = "foobar";
			changes.Should().Equal(new object[] {"ProxyPassword"});

			model.ProxyPassword = "blub";
			changes.Should().Equal(new object[]
				{
					"ProxyPassword",
					"ProxyPassword"
				});

			model.ProxyPassword = "blub";
			changes.Should().Equal(new object[]
				{
					"ProxyPassword",
					"ProxyPassword"
				});
		}

		[Test]
		public void TestProxyPassword2()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsMainPanelViewModel(settings);

			model.ProxyPassword = "foobar";
			settings.AutoUpdate.ProxyPassword.Should().Be("foobar");

			model.ProxyPassword = null;
			settings.AutoUpdate.ProxyPassword.Should().BeNull();

			model.ProxyPassword = string.Empty;
			settings.AutoUpdate.ProxyPassword.Should().BeEmpty();
		}
	}
}