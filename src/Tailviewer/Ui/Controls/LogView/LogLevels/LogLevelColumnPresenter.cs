using System;
using System.Globalization;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView.LogLevels
{
	public sealed class LogLevelColumnPresenter
		: AbstractLogColumnPresenter<LevelFlags>
	{
		public LogLevelColumnPresenter(TextSettings textSettings)
			: base(Columns.LogLevel, textSettings)
		{}

		#region Overrides of AbstractLogColumnPresenter<LevelFlags>

		protected override void UpdateWidth(ILogFile logFile, TextSettings textSettings)
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
