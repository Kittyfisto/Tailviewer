using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer
{
	public static class Changelog
	{
		private static readonly List<Change> AllChanges;

		public static Change MostRecent
		{
			get { return AllChanges.Last(); }
		}

		public static IEnumerable<Change> Changes
		{
			get { return AllChanges; }
		}

		static Changelog()
		{
			AllChanges = new List<Change>();

			AddV0286();
			AddMostRecent();
		}

		private static void AddMostRecent()
		{
			var features = new[]
			{
				"Search results are properly highlighted",
				"Search results are always brought into view"
			};
			var bugfixes = new string[] {};
			var misc = new[]
				{
					"Added changelog",
					"Unhandled errors are logged to action center"
				};
			var change = new Change(Constants.BuildDate, Constants.ApplicationVersion, features, bugfixes, misc);
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