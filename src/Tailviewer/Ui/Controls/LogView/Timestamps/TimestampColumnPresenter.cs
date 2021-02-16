using System;
using System.Globalization;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Columns;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView.Timestamps
{
	public sealed class TimestampColumnPresenter
		: AbstractLogColumnPresenter<DateTime?>
	{
		public TimestampColumnPresenter(TextSettings textSettings)
			: base(LogColumns.Timestamp, textSettings)
		{
		}

		#region Overrides of AbstractLogColumnPresenter<DateTime?>

		protected override void UpdateWidth(ILogSource logSource, TextSettings textSettings)
		{
			// I guess it is fair to assume that dates have the same length, no matter which date we're formatting.
			var value = new DateTime(1970, 1, 1, 0, 0, 0);
			var culture = CultureInfo.CurrentCulture;

			Width = textSettings.EstimateWidthUpperLimit(TimestampFormatter.ToString(value, culture));
		}

		protected override AbstractLogEntryValueFormatter CreateFormatter(DateTime? value)
		{
			return new TimestampFormatter(value, TextSettings);
		}

		#endregion
	}
}
