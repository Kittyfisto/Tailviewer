using System;
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

			Add0280();
		}

		private static void Add0280()
		{
			var features = new[]
				{
					"Added action/notification center",
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
			var change = new Change(new Version(0, 2, 81), features, bugfixes, misc);
			AllChanges.Add(change);
		}
	}
}