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
		/// Returns all changesets (i.e. effectively all patches) of the latest minor version.
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

		public static Change MostRecent
		{
			get { return AllChanges.Last(); }
		}

		public static IReadOnlyList<Change> Changes
		{
			get { return AllChanges; }
		}

		static Changelog()
		{
			AllChanges = new List<Change>();

			AddV0286();
			AddV030();
			AddV031();
			AddV032();
			AddV033();
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
			var version = new Version(0, 3, 3);
			var change = new Change(Constants.BuildDate, version, features, bugfixes, misc);
			AllChanges.Add(change);
		}

		private static void AddV032()
		{
			var features = new string[]
			{
			};
			var bugfixes = new[]
			{
				"Fixed bug causing diplication of lines"
			};
			var misc = new string[]
			{
			};
			var releaseDate = new DateTime(2017, 3, 16);
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