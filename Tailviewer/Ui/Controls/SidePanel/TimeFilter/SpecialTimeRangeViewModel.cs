using System.Collections.Generic;
using Tailviewer.Core.Settings;

namespace Tailviewer.Ui.Controls.SidePanel.TimeFilter
{
	public sealed class SpecialTimeRangeViewModel
		: IChoseTimeRangeViewModel
	{
		private static readonly IReadOnlyDictionary<SpecialTimeRange, string> Titles;
		private readonly SpecialTimeRange _range;

		public SpecialTimeRange Range => _range;

		private readonly string _title;

		public string Title => _title;

		static SpecialTimeRangeViewModel()
		{
			Titles = new Dictionary<SpecialTimeRange, string>
			{
				{SpecialTimeRange.Today, "Today"},
				{SpecialTimeRange.ThisWeek, "This week"},
				{SpecialTimeRange.ThisMonth, "This month"},
				{SpecialTimeRange.ThisYear, "This year"},
				{SpecialTimeRange.Last24Hours, "Last 24 hours"},
				{SpecialTimeRange.Last7Days, "Last 7 days"},
				{SpecialTimeRange.Last30Days, "Last 30 days"},
				{SpecialTimeRange.Last365Days, "Last 365 days"}
			};
		}

		public SpecialTimeRangeViewModel(SpecialTimeRange range)
		{
			_range = range;
			Titles.TryGetValue(range, out _title);
		}
	}
}