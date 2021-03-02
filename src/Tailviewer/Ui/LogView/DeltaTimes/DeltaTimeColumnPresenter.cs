using System;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.DeltaTimes
{
	/// <summary>
	///     A "canvas" which draws the elapsed time to the previous log entry in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="IReadOnlyLogEntry.RawContent" />.
	/// </summary>
	public sealed class DeltaTimeColumnPresenter
		: AbstractLogColumnPresenter<TimeSpan?>
	{
		public DeltaTimeColumnPresenter(TextSettings textSettings)
			: base(GeneralColumns.DeltaTime, textSettings)
		{
			Width = 50;
		}

		protected override void UpdateWidth(ILogSource logSource, TextSettings textSettings)
		{}

		protected override AbstractLogEntryValueFormatter CreateFormatter(TimeSpan? value)
		{
			return new DeltaTimeFormatter(value, TextSettings);
		}
	}
}