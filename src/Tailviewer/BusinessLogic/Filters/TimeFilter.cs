using System;
using System.Diagnostics.Contracts;
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

		public TimeFilterMode Mode
		{
			get { return _settings.Mode; }
			set { _settings.Mode = value; }
		}

		public SpecialDateTimeInterval SpecialInterval
		{
			get { return _settings.SpecialInterval; }
			set { _settings.SpecialInterval = value; }
		}

		public DateTime? Minimum
		{
			get { return _settings.Minimum; }
			set { _settings.Minimum = value; }
		}

		public DateTime? Maximum
		{
			get { return _settings.Maximum; }
			set { _settings.Maximum = value; }
		}

		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			return _settings.CreateFilter();
		}
	}
}