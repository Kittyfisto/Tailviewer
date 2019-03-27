using System.Collections.Generic;
using Tailviewer.Core.Settings;

namespace Tailviewer.Ui.Controls.SidePanel.TimeFilter
{
	public sealed class SpecialTimeRangeViewModel
		: IChoseTimeRangeViewModel
	{
		private static readonly IReadOnlyDictionary<SpecialDateTimeInterval, string> Titles;
		private readonly SpecialDateTimeInterval _interval;

		public SpecialDateTimeInterval Interval => _interval;

		private readonly string _title;

		public string Title => _title;

		static SpecialTimeRangeViewModel()
		{
			Titles = new Dictionary<SpecialDateTimeInterval, string>
			{
				{SpecialDateTimeInterval.Today, "Today"},
				{SpecialDateTimeInterval.ThisWeek, "This week"},
				{SpecialDateTimeInterval.ThisMonth, "This month"},
				{SpecialDateTimeInterval.ThisYear, "This year"},
				{SpecialDateTimeInterval.Last24Hours, "Last 24 hours"},
				{SpecialDateTimeInterval.Last7Days, "Last 7 days"},
				{SpecialDateTimeInterval.Last30Days, "Last 30 days"},
				{SpecialDateTimeInterval.Last365Days, "Last 365 days"}
			};
		}

		public SpecialTimeRangeViewModel(SpecialDateTimeInterval interval)
		{
			_interval = interval;
			Titles.TryGetValue(interval, out _title);
		}
	}
}