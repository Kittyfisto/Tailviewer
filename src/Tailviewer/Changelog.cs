using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer
{
	public static class Changelog
	{
		private static readonly List<Change> AllChanges;

		/// <summary>
		///     Returns all changesets (i.e. effectively all patches) of the latest minor version.
		/// </summary>
		public static IEnumerable<Change> MostRecentPatches
		{
			get
			{
				var version = MostRecent.Version;
				return AllChanges.Where(x => x.Version.Major == version.Major &&
				                             x.Version.Minor == version.Minor)
					.ToList();
			}
		}

		public static Change MostRecent => AllChanges.Last();

		public static IReadOnlyList<Change> Changes => AllChanges;

		static Changelog()
		{
			AllChanges = new List<Change>();

			AddV0286();
			AddV030();
			AddV031();
			AddV032();
			AddV033();
			AddV040();
			AddV041();
			AddV042();
			AddV050();
			AddV051();
			AddV052();
			AddV060();
			AddV061();
			AddV062();
			AddV070();
			AddV071();
			AddV080();
			AddV081();
			AddV090();
			AddV091();
			AddV092();
			AddV093();
			AddV094();
		}

		private static void AddV094()
		{
			var features = new string[]
			{};
			var bugfixes = new string[]
			{};
			var misc = new []
			{
				"Added shortcut for new bookmark (Ctrl+B) [#240](https://github.com/Kittyfisto/Tailviewer/issues/240)"
			};
			var releaseDate = new DateTime(2020, 8, 20);
			var version = new Version(0, 9, 4);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV093()
		{
			var features = new[]
			{
				"Fully customized color settings for log levels [#216](https://github.com/Kittyfisto/Tailviewer/issues/216)"
			};
			var bugfixes = new string[]
				{};
			var misc = new[]
			{
				"Add setting for tab size [#232](https://github.com/Kittyfisto/Tailviewer/issues/232)",
			};
			var releaseDate = new DateTime(2020, 7, 21);
			var version = new Version(0, 9, 3);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV092()
		{
			var features = new string[]
			{};
			var bugfixes = new string[]
			{};
			var misc = new[]
			{
				"Implemented uninstallation [#219](https://github.com/Kittyfisto/Tailviewer/issues/219)"
			};
			var releaseDate = new DateTime(2020, 3, 17);
			var version = new Version(0, 9, 2);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV091()
		{
			var features = new[]
			{
				"Introduced plugin browser which works in conjunction with self hosted repositories [#90](https://github.com/Kittyfisto/Tailviewer/issues/90)",
				"Introduced clear screen & display [#215](https://github.com/Kittyfisto/Tailviewer/issues/215)"
			};
			var bugfixes = new string[]
			{
			};
			var misc = new string[]
			{
			};
			var releaseDate = new DateTime(2020, 3, 3);
			var version = new Version(0, 9, 1);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV090()
		{
			var features = new[]
			{
				"Introduced find all search (ctrl+shift+f) [#210](https://github.com/Kittyfisto/Tailviewer/issues/210)",
				"Introduced plugin browser which works on conjunction with self hosted repositories [#90](https://github.com/Kittyfisto/Tailviewer/issues/90)",
				"Introduced overview side panel for a condensed view onto the log file [#207](https://github.com/Kittyfisto/Tailviewer/issues/207)",
				"Introduced issues side panel for automated log file analysis [#209](https://github.com/Kittyfisto/Tailviewer/issues/209)",
				"Introduced clear screen & display [#215](https://github.com/Kittyfisto/Tailviewer/issues/215)"
			};
			var bugfixes = new[]
			{
				"Fixed crash upon startup when bookmarks.xml was corrupted [#214](https://github.com/Kittyfisto/Tailviewer/issues/214)",
				"Fixed filtering/searching of text which includes a tab character [#213](https://github.com/Kittyfisto/Tailviewer/issues/213)",
				"Fixed closing the last data source [#223](https://github.com/Kittyfisto/Tailviewer/issues/223)",
				"Fixed display of multiline log files [#221](https://github.com/Kittyfisto/Tailviewer/issues/221)",
				"Fixed merged log file data source name in case filters are used [#154](https://github.com/Kittyfisto/Tailviewer/issues/154)"
			};
			var misc = new[]
			{
				"The titlebar properly reacts to losing focus [#202](https://github.com/Kittyfisto/Tailviewer/issues/202)", // 
				"The left side panel can be collapsed [#201](https://github.com/Kittyfisto/Tailviewer/issues/201)"
			};
			var releaseDate = new DateTime(2019, 10, 31);
			var version = new Version(0, 9, 0);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV081()
		{
			var features = new[]
			{
				"Added folder data sources [#125](https://github.com/Kittyfisto/Tailviewer/issues/125)",
				"Font size is now configurable [#53](https://github.com/Kittyfisto/Tailviewer/issues/53)",
			};
			var bugfixes = new[]
			{
				"Proxy password box no longer captures focus indefinitely [#188](https://github.com/Kittyfisto/Tailviewer/issues/188)",
			};
			var misc = new[]
			{
				"A notification becomes visible when a plugin is used to display a log file [#172](https://github.com/Kittyfisto/Tailviewer/issues/172)",
				"Bookmarks are stored/restored in between sessions [#66](https://github.com/Kittyfisto/Tailviewer/issues/66)",
				"Titlebar contains more information about selected files [#127](https://github.com/Kittyfisto/Tailviewer/issues/127)",
			};
			var releaseDate = new DateTime(2019, 06, 23);
			var version = new Version(0, 8, 1);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV080()
		{
			var features = new string[]
			{
			};
			var bugfixes = new[]
			{
				"Fixed rendering very long lines",
				"Fixed crash starting Tailviewer",
				"Fixed bug where merged data source sometimes displayed the wrong source file name",
				"Fixed multi line detection for log files without log levels",
				"Merged data source appear in data source quick chooser (ctrl+t)",
			};
			var misc = new []
			{
				"Improved performance of merged data sources",
				"Opening the quick filter panel ensures that there is always at least one empty quick filter present for quick access",
			};
			var releaseDate = new DateTime(2019, 05, 29);
			var version = new Version(0, 8, 0);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV071()
		{
			var features = new[]
			{
				"Navigate to data source (CTRL+T)",
				"Go to line (CTRL+G)",
				"Added \"delta times\" column"
			};
			var bugfixes = new[]
			{
				"Fixed merging multiline files",
				"Fixed item selection after drag 'n drop",
				"Action center doesn't crash after export anymore"
			};
			var misc = new[]
			{
				"Changed default scrollspeed to 2 lines per wheel tick",
				"Scrollspeed can be adjusted in settings"
			};
			var releaseDate = new DateTime(2017, 12, 1);
			var version = new Version(0, 7, 1);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV070()
		{
			var features = new[]
			{
				"Introduced file name column for merged data sources"
			};
			var bugfixes = new[]
			{
				"Fixed last written age display for values > 30 days"
			};
			var misc = new[]
			{
				"Merged data sources can be renamed",
				"Merged data source age, size is updated more frequently",
				"Introduced Trace level toggle",
				"Debug/Trace messages are display in gray"
			};
			var releaseDate = new DateTime(2017, 11, 22);
			var version = new Version(0, 7, 0);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV062()
		{
			var features = new[]
			{
				"Tailviewer plugins can be packed into one file",
				"Multiple versions of the same plugin coexist (latest supported will be loaded)"
			};
			var bugfixes = new[]
			{
				"Fixed error message when a log file cannot be accessed"
			};
			var misc = new[]
			{
				"Introduced progress bar to show status of log file reading/filtering",
				"Moved settings, plugins and info to the bottom left",
				"Installed plugins page is prettier"
			};
			var releaseDate = new DateTime(2017, 9, 13);
			var version = new Version(0, 6, 2);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV061()
		{
			var features = new[]
			{
				"Tailviewer can now be enhanced by plugins"
			};
			var bugfixes = new string[]
			{
				
			};
			var misc = new[]
			{
				"Improved data sources tree look",
				"Changing a filter value automatically enables it"
			};
			var releaseDate = new DateTime(2017, 7, 19);
			var version = new Version(0, 6, 1);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV060()
		{
			var features = new[]
			{
				"Tailviewer can only be started once (2nd time focuses the original window)",
				"Filtered log files show actual line numbers"
			};
			var bugfixes = new[]
			{
				"Placing bookmarks on a filtered data source works as expected",
				"The full path of the currently opened data source is visible after app start again",
				"Fixed flickering when displaying a log file without (discernible) timestamps",
				"Drastically improved performance of reading log files without (discernible) timestamps"
			};
			var misc = new[]
			{
				"Tailviewer's log file can be opened from the context menu"
			};
			var releaseDate = new DateTime(2017, 7, 13);
			var version = new Version(0, 6, 0);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV052()
		{
			var features = new string[]
			{
			};
			var bugfixes = new string[]
			{};
			var misc = new[]
			{
				"More keyboard shortcuts"
			};
			var releaseDate = new DateTime(2017, 6, 19);
			var version = new Version(0, 5, 2);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV051()
		{
			var features = new string[]
			{
			};
			var bugfixes = new []
			{
				"Fixed wrong parsing of certain timestamps",
				"Data sources side panel accepts drag 'n drop from explorer"
			};
			var misc = new[]
			{
				"Export folder is configurable",
				"Added tooltips to most UI elements"
			};
			var releaseDate = new DateTime(2017, 5, 22);
			var version = new Version(0, 5, 1);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV050()
		{
			var features = new[]
			{
				"Introduced bookmarks"
			};
			var bugfixes = new[]
			{
				"'Last written' is calculated from timestamps in log file if LastWriteTime happens to be unreliable"
			};
			var misc = new[]
			{
				"Added support for more timestamp formats",
				"Added proper explanation when merged data source is empty",
				"More colorful design",
				"Improved 'Open in explorer' button icon"
			};
			var releaseDate = new DateTime(2017, 5, 10);
			var version = new Version(0, 5, 0);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV042()
		{
			var features = new[]
			{
				"Added export to file"
			};
			var bugfixes = new[]
			{
				"Fixed horizontal scrollbar repeat buttons",
			};
			var misc = new string[]
			{
				
			};
			var releaseDate = new DateTime(2017, 5, 4);
			var version = new Version(0, 4, 2);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV041()
		{
			var features = new string[]
			{
				
			};
			var bugfixes = new[]
			{
				"Fixed open in explorer button for merged data sources",
				"Fixed empty data source appearing as not existing data source"
			};
			var misc = new[]
			{
				"Added always on top setting",
				"Data source order is persisted",
				"Collapse of merged data source is persisted",
				"# of active filters is shown in side panel",
				"Added installed programs entry"
			};
			var releaseDate = new DateTime(2017, 5, 2);
			var version = new Version(0, 4, 1);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV040()
		{
			var features = new[]
			{
				"Introduced new design"
			};
			var bugfixes = new string[]
			{
				
			};
			var misc = new string[]
			{};
			var releaseDate = new DateTime(2017, 5, 1);
			var version = new Version(0, 4, 0);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV033()
		{
			var features = new string[]
			{
			};
			var bugfixes = new[]
			{
				"Fixed bug causing a single line to be interpreted as many lines",
				"Fixed bug causing a file to be shown as empty even when it was not"
			};
			var misc = new string[]
			{
			};
			var releaseDate = new DateTime(2017, 3, 16);
			var version = new Version(0, 3, 3);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV032()
		{
			var features = new string[]
			{
			};
			var bugfixes = new[]
			{
				"Fixed bug causing duplication of lines"
			};
			var misc = new string[]
			{
			};
			var releaseDate = new DateTime(2017, 3, 15);
			var version = new Version(0, 3, 2);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV031()
		{
			var features = new[]
			{
				"Added single/multi line toggle"
			};
			var bugfixes = new[]
			{
				"Fixed degrading performance over time when filtering",
				"Fixed memory leak when filtering"
			};
			var misc = new[]
			{
				"Improved Scrollbar usability",
				"Added alternating row colors"
			};
			var releaseDate = new DateTime(2017, 3, 14);
			var version = new Version(0, 3, 1);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV030()
		{
			var features = new[]
			{
				"Search results are properly highlighted",
				"Search results are always brought into view",
				"Added toggle to hide empty lines"
			};
			var bugfixes = new string[] {};
			var misc = new[]
				{
					"Added changelog",
					"Unhandled errors are logged to action center"
				};
			var releaseDate = new DateTime(2017, 1, 16);
			var version = new Version(0, 3, 0);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV0286()
		{
			var features = new[]
				{
					"Added action/notification center",
					"Errors that occur while checking for updates are displayed in action center"
				};
			var bugfixes = new[]
				{
					"Fixed UI freeze when data source on network wasn't reachable anymore",
					"Fixed crash in data source tree",
					"Fixed data sources being hard to select"
				};
			var misc = new[]
				{
					"Changed scrollbar style to 'flat'",
					"Changed window style to chromeless"
				};
			var releaseDate = new DateTime(2016, 10, 17);
			var version = new Version(0, 2, 86);
			var change = new Change(releaseDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}
	}
}
