using System;
using System.Globalization;
using System.Linq;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.LogLevels
{
	public sealed class LogLevelColumnPresenter
		: AbstractLogColumnPresenter<LevelFlags>
	{
		public LogLevelColumnPresenter(TextSettings textSettings)
			: base(GeneralColumns.LogLevel, textSettings)
		{}

		#region Overrides of AbstractLogColumnPresenter<LevelFlags>

		protected override void UpdateWidth(ILogSource logSource, TextSettings textSettings)
		{
			var culture = CultureInfo.CurrentCulture;
			var values = Enum.GetValues(typeof(LevelFlags)).Cast<LevelFlags>().ToList();
			var width = values.Max(x => textSettings.EstimateWidthUpperLimit(LevelFormatter.ToString(x, culture)));
			Width = width;
		}

		protected override AbstractLogEntryValueFormatter CreateFormatter(LevelFlags value)
		{
			return new LevelFormatter(value, TextSettings);
		}

		#endregion
	}
}
