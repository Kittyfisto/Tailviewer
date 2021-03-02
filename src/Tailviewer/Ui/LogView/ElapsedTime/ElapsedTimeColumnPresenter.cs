using System;
using System.Globalization;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.ElapsedTime
{
	public sealed class ElapsedTimeColumnPresenter
		: AbstractLogColumnPresenter<TimeSpan?>
	{
		public ElapsedTimeColumnPresenter(TextSettings textSettings)
			: base(Columns.ElapsedTime, textSettings)
		{
		}

		protected override void UpdateWidth(ILogSource logSource, TextSettings textSettings)
		{
			if (logSource != null)
			{
				var culture = CultureInfo.CurrentCulture;
				var maximum = ElapsedTimeFormatter.ToString(logSource.GetProperty(Properties.EndTimestamp) - logSource.GetProperty(Properties.StartTimestamp), culture);
				var maximumWidth = textSettings.EstimateWidthUpperLimit(maximum);
				Width = maximumWidth;
			}
			else
			{
				Width = 0;
			}
		}

		protected override AbstractLogEntryValueFormatter CreateFormatter(TimeSpan? value)
		{
			return new ElapsedTimeFormatter(value, TextSettings);
		}
	}
}