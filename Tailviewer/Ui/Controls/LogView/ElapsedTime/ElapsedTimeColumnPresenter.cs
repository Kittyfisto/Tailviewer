using System;
using System.Globalization;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Ui.Controls.LogView.ElapsedTime
{
	public sealed class ElapsedTimeColumnPresenter
		: AbstractLogColumnPresenter<TimeSpan?>
	{
		public ElapsedTimeColumnPresenter()
			: base(LogFileColumns.ElapsedTime)
		{
		}

		protected override void UpdateWidth(ILogFile logFile)
		{
			var culture = CultureInfo.CurrentCulture;
			var maximum = ElapsedTimePresenter.ToString(logFile.EndTimestamp - logFile.StartTimestamp, culture);
			var maximumWidth = TextHelper.EstimateWidthUpperLimit(maximum);
			Width = maximumWidth;
		}

		protected override AbstractLogEntryValuePresenter CreatePresenter(TimeSpan? value)
		{
			return new ElapsedTimePresenter(value);
		}
	}
}