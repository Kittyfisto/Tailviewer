using System;
using System.Diagnostics.Contracts;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.Filters
{
	internal sealed class QuickFilter
	{
		private readonly Settings.QuickFilter _settings;

		public QuickFilter(Settings.QuickFilter settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
		}

		public string Value
		{
			get { return _settings.Value; }
			set { _settings.Value = value; }
		}

		public bool IgnoreCase
		{
			get { return _settings.IgnoreCase; }
			set { _settings.IgnoreCase = value; }
		}

		public bool IsInverted
		{
			get { return _settings.IsInverted; }
			set { _settings.IsInverted = value; }
		}

		public QuickFilterMatchType MatchType
		{
			get { return _settings.MatchType; }
			set { _settings.MatchType = value; }
		}

		public Guid Id
		{
			get { return _settings.Id; }
		}

		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			string value = Value;
			ILogEntryFilter filter = null;
			switch (MatchType)
			{
				case QuickFilterMatchType.StringFilter:
					if (!string.IsNullOrEmpty(value))
						filter = new SubstringFilter(value, IgnoreCase);
					break;

				case QuickFilterMatchType.WildcardFilter:
					if (!string.IsNullOrEmpty(value))
						filter = new WildcardFilter(value, IgnoreCase);
					break;

				case QuickFilterMatchType.RegexpFilter:
					if (!string.IsNullOrEmpty(value))
						filter = new RegexFilter(value, IgnoreCase);
					break;
			}

			if (filter != null && IsInverted)
				filter = new InvertFilter(filter);

			return filter;
		}
	}
}