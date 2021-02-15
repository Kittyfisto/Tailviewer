using System;
using System.Globalization;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView.ElapsedTime
{
	public sealed class ElapsedTimeColumnPresenter
		: AbstractLogColumnPresenter<TimeSpan?>
	{
		public ElapsedTimeColumnPresenter(TextSettings textSettings)
			: base(Columns.ElapsedTime, textSettings)
		{
		}

		protected override void UpdateWidth(ILogFile logFile, TextSettings textSettings)
		{
			if (logFile != null)
			{
				var culture = CultureInfo.CurrentCulture;
				var maximum = ElapsedTimeFormatter.ToString(logFile.GetProperty(Core.LogFiles.Properties.EndTimestamp) - logFile.GetProperty(Core.LogFiles.Properties.StartTimestamp), culture);
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