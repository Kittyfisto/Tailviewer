using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer
{
	public static class ChangeLog
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

		static ChangeLog()
		{
			AllChanges = new List<Change>();

			AddMostRect();
		}

		private static void AddMostRect()
		{
			var features = new[]
				{
					"Added action/notification center",
					"Errors that occur while checking for updates are displayed in action center"
				};
			var bugfixes = new[]
				{
					"Fixed UI freeze data source on network wasn't reachable anymore",
					"Fixed crash in data source tree",
					"Fixed data sources being hard to select"
				};
			var misc = new[]
				{
					"Changed scrollbar style to 'flat'",
					"Changed window style to chromeless"
				};
			var change = new Change(Constants.ApplicationVersion, features, bugfixes, misc);
			AllChanges.Add(change);
		}
	}
}