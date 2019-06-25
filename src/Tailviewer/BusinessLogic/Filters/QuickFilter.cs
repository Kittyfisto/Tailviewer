using System;
using System.Diagnostics.Contracts;
using Tailviewer.Core;
using Tailviewer.Core.Settings;

namespace Tailviewer.BusinessLogic.Filters
{
	public sealed class QuickFilter
	{
		private readonly Core.Settings.QuickFilter _settings;

		public QuickFilter(Core.Settings.QuickFilter settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

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

		public FilterMatchType MatchType
		{
			get { return _settings.MatchType; }
			set { _settings.MatchType = value; }
		}

		public QuickFilterId Id => _settings.Id;

		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			return _settings.CreateFilter();
		}
	}
}