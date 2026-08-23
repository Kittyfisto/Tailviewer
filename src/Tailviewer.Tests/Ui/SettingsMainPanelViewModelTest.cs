using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Ui;
using Tailviewer.Ui.Settings;

namespace Tailviewer.Tests.Ui
{
	[TestFixture]
	public sealed class SettingsMainPanelViewModelTest
	{
		[Test]
		public void TestConstruction1([Values(true, false)] bool recursive)
		{
			var settings = new ApplicationSettings("foo");
			settings.DataSources.FolderDataSourceRecursive = recursive;
			settings.DataSources.FolderDataSourcePattern = "*.log;*.txt";

			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			model.FolderDataSourceRecursive.Should().Be(recursive);
			model.FolderDataSourcePatterns.Should().Be("*.log;*.txt");
		}

		[Test]
		public void TestRestoreDefaultColors()
		{
			var settings = new ApplicationSettings("foo");
			settings.AllowSave = false;
			settings.Ui.ThemeColor = Colors.Red;
			settings.Ui.ThemeMode = ThemeMode.Dark;

			foreach (var level in new[]
			{
				settings.LogViewer.Other,
				settings.LogViewer.Trace,
				settings.LogViewer.Debug,
				settings.LogViewer.Info,
				settings.LogViewer.Warning,
				settings.LogViewer.Error,
				settings.LogViewer.Fatal
			})
			{
				level.IsCustom = true;
				level.ForegroundColor = Colors.Pink;
				level.BackgroundColor = Colors.Blue;
			}

			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());
			try
			{
				model.RestoreDefaultColorsCommand.Execute(null);

				settings.Ui.ThemeColor.Should().Be(UISettings.DefaultThemeColor);

				AssertReset(settings.LogViewer.Other, model.OtherLevel, LogLevelDefaults.Light.Other, LogLevelDefaults.Dark.Other);
				AssertReset(settings.LogViewer.Trace, model.TraceLevel, LogLevelDefaults.Light.Trace, LogLevelDefaults.Dark.Trace);
				AssertReset(settings.LogViewer.Debug, model.DebugLevel, LogLevelDefaults.Light.Debug, LogLevelDefaults.Dark.Debug);
				AssertReset(settings.LogViewer.Info, model.InfoLevel, LogLevelDefaults.Light.Info, LogLevelDefaults.Dark.Info);
				AssertReset(settings.LogViewer.Warning, model.WarningLevel, LogLevelDefaults.Light.Warning, LogLevelDefaults.Dark.Warning);
				AssertReset(settings.LogViewer.Error, model.ErrorLevel, LogLevelDefaults.Light.Error, LogLevelDefaults.Dark.Error);
				AssertReset(settings.LogViewer.Fatal, model.FatalLevel, LogLevelDefaults.Light.Fatal, LogLevelDefaults.Dark.Fatal);
			}
			finally
			{
				ThemeManager.Apply(UISettings.DefaultThemeColor, ThemeMode.Light);
			}
		}

		private static void AssertReset(LogLevelSettings settings, LogLevelSettingsViewModel viewModel, LogLevelSettings light, LogLevelSettings dark)
		{
			settings.IsCustom.Should().BeFalse("because the reset must restore the theme-following state");
			settings.ForegroundColor.Should().Be(light.ForegroundColor);
			settings.BackgroundColor.Should().Be(light.BackgroundColor);

			viewModel.ForegroundColor.Should().Be(dark.ForegroundColor);
			viewModel.BackgroundColor.Should().Be(dark.BackgroundColor);
		}

		public static IEnumerable<Encoding> Encodings => new[] {null, Encoding.Default, Encoding.UTF7, Encoding.UTF8};

		[Test]
		public void TestConstruction2([ValueSource(nameof(Encodings))] Encoding encoding)
		{
			var settings = new ApplicationSettings("foo");
			settings.LogFile.DefaultEncoding = encoding;

			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			model.DefaultTextFileEncoding.Should().NotBeNull();
			model.DefaultTextFileEncoding.Encoding.Should().BeSameAs(encoding);
		}

		[Test]
		public void TestSetFolderDataSourcePatterns1([Values(null, "", " ", ";;")] string emptyPattern)
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			model.FolderDataSourcePatterns = emptyPattern;
			settings.DataSources.FolderDataSourcePattern.Should().Be(emptyPattern);
		}

		[Test]
		public void TestSetFolderDataSourcePatternsSkipTrailingSpace()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			model.FolderDataSourcePatterns = "*.log ";
			settings.DataSources.FolderDataSourcePattern.Should().Be("*.log ");
		}

		[Test]
		public void TestSetFolderDataSourceMultiplePatterns()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			model.FolderDataSourcePatterns = "*.txt;*.log ;*log*";
			settings.DataSources.FolderDataSourcePattern.Should().Be("*.txt;*.log ;*log*");
		}

		[Test]
		public void TestSetFolderDataSourcePatternsSkipLeadingSpace()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			model.FolderDataSourcePatterns = " *.log";
			settings.DataSources.FolderDataSourcePattern.Should().Be(" *.log");
		}

		[Test]
		public void TestProxyPassword1()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

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
			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			model.ProxyPassword = "foobar";
			settings.AutoUpdate.ProxyPassword.Should().Be("foobar");

			model.ProxyPassword = null;
			settings.AutoUpdate.ProxyPassword.Should().BeNull();

			model.ProxyPassword = string.Empty;
			settings.AutoUpdate.ProxyPassword.Should().BeEmpty();
		}

		[Test]
		public void TestTestChangeDefaultEncoding()
		{
			var settings = new ApplicationSettings("foo");
			var model = new SettingsFlyoutViewModel(settings, new ServiceContainer());

			var newEncoding = model.TextFileEncodings.Last();
			model.DefaultTextFileEncoding = newEncoding;
			settings.LogFile.DefaultEncoding.Should().Be(newEncoding.Encoding);

			newEncoding = model.TextFileEncodings.First();
			model.DefaultTextFileEncoding = newEncoding;
			settings.LogFile.DefaultEncoding.Should().Be(newEncoding.Encoding);
		}
	}
}
