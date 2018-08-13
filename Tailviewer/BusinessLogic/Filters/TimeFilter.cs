using System;
using System.Diagnostics.Contracts;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Settings;

namespace Tailviewer.BusinessLogic.Filters
{
	public sealed class TimeFilter
	{
		private readonly Core.Settings.TimeFilter _settings;

		public TimeFilter(Core.Settings.TimeFilter settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			_settings = settings;
		}

		public SpecialTimeRange? Range
		{
			get { return _settings.Range; }
			set { _settings.Range = value; }
		}

		public DateTime? Start
		{
			get { return _settings.Start; }
			set { _settings.Start = value; }
		}

		public DateTime? End
		{
			get { return _settings.End; }
			set { _settings.End = value; }
		}

		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			return _settings.CreateFilter();
		}
	}
}