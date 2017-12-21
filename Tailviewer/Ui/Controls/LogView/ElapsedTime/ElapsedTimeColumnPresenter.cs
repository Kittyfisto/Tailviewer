using System;
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
			Width = TextHelper.EstimateWidthUpperLimit(ElapsedTimePresenter.CharacterWidth);
		}

		protected override AbstractLogEntryValuePresenter CreatePresenter(TimeSpan? value)
		{
			return new ElapsedTimePresenter(value);
		}
	}
}