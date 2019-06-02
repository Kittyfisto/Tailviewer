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
			: base(LogFileColumns.ElapsedTime, textSettings)
		{
		}

		protected override void UpdateWidth(ILogFile logFile, TextSettings textSettings)
		{
			var culture = CultureInfo.CurrentCulture;
			var maximum = ElapsedTimePresenter.ToString(logFile.GetValue(LogFileProperties.EndTimestamp) - logFile.GetValue(LogFileProperties.StartTimestamp), culture);
			var maximumWidth = textSettings.EstimateWidthUpperLimit(maximum);
			Width = maximumWidth;
		}

		protected override AbstractLogEntryValuePresenter CreatePresenter(TimeSpan? value)
		{
			return new ElapsedTimePresenter(value, TextSettings);
		}
	}
}