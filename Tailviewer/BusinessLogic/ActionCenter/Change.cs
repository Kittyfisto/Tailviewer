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
		private readonly Version _version;

		public Change(Version version, IEnumerable<string> features, IEnumerable<string> bugfixes, IEnumerable<string> misc)
		{
			_version = version;
			_features = new List<string>(features);
			_bugfixes = new List<string>(bugfixes);
			_misc = new List<string>(misc);
		}

		public Version Version
		{
			get { return _version; }
		}

		public IEnumerable<string> Features
		{
			get { return _features; }
		}

		public IEnumerable<string> Bugfixes
		{
			get { return _bugfixes; }
		}

		public IEnumerable<string> Misc
		{
			get { return _misc; }
		}

		public string Title
		{
			get { return string.Format("What's new in v{0}?", Version.Format()); }
		}
	}
}