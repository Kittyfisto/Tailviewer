using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView.DeltaTimes
{
	/// <summary>
	///     A "canvas" which draws the elapsed time to the previous log entry in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed class DeltaTimeColumnPresenter
		: AbstractLogColumnPresenter<TimeSpan?>
	{
		public DeltaTimeColumnPresenter(TextSettings textSettings)
			: base(Columns.DeltaTime, textSettings)
		{
			Width = 50;
		}

		protected override void UpdateWidth(ILogFile logFile, TextSettings textSettings)
		{}

		protected override AbstractLogEntryValueFormatter CreateFormatter(TimeSpan? value)
		{
			return new DeltaTimeFormatter(value, TextSettings);
		}
	}
}