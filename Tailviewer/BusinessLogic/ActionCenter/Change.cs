using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	/// <summary>
	///     Represents the changes made in a specific release version.
	/// </summary>
	public sealed class Change
		: INotification
	{
		private readonly List<string> _bugfixes;
		private readonly List<string> _features;
		private readonly List<string> _misc;

		public Change(DateTime releaseDate, Version version, IEnumerable<string> features, IEnumerable<string> bugfixes,
			IEnumerable<string> misc)
		{
			ReleaseDate = releaseDate;
			Version = version;
			_features = new List<string>(features);
			_bugfixes = new List<string>(bugfixes);
			_misc = new List<string>(misc);
		}

		public DateTime ReleaseDate { get; }

		public Version Version { get; }

		public IEnumerable<string> Features => _features;

		public IEnumerable<string> Bugfixes => _bugfixes;

		public IEnumerable<string> Misc => _misc;

		public string Title => string.Format("What's new in v{0}?", Version.ToString(2));

		/// <summary>
		/// </summary>
		/// <remarks>
		///     The changelog is just not interesting enough to immediately show to the user.
		///     Maybe if we keep track of whether or not the user has closed this window...
		/// </remarks>
		public bool ForceShow => false;

		public static Change Merge(IEnumerable<Change> changes)
		{
			var features = new List<string>();
			var bugfixes = new List<string>();
			var misc = new List<string>();
			var version = new Version();
			var releaseDate = DateTime.MinValue;
			foreach (var change in changes)
			{
				features.AddRange(change.Features);
				bugfixes.AddRange(change.Bugfixes);
				misc.AddRange(change.Misc);
				if (change.Version > version)
				{
					version = change.Version;
					releaseDate = change.ReleaseDate;
				}
			}
			return new Change(releaseDate, version, features, bugfixes, misc);
		}
	}
}