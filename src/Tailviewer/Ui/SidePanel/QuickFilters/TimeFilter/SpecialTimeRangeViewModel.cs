using System.Collections.Generic;
using Tailviewer.Core;

using Tailviewer.Localization;

namespace Tailviewer.Ui.SidePanel.QuickFilters.TimeFilter
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
				{SpecialDateTimeInterval.Today, Strings.Today},
				{SpecialDateTimeInterval.ThisWeek, Strings.ThisWeek},
				{SpecialDateTimeInterval.ThisMonth, Strings.ThisMonth},
				{SpecialDateTimeInterval.ThisYear, Strings.ThisYear},
				{SpecialDateTimeInterval.Last24Hours, Strings.Last24Hours},
				{SpecialDateTimeInterval.Last7Days, Strings.Last7Days},
				{SpecialDateTimeInterval.Last30Days, Strings.Last30Days},
				{SpecialDateTimeInterval.Last365Days, Strings.Last365Days}
			};
		}

		public SpecialTimeRangeViewModel(SpecialDateTimeInterval interval)
		{
			_interval = interval;
			Titles.TryGetValue(interval, out _title);
		}
	}
}