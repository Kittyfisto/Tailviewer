namespace Tailviewer.Settings
{
	/// <summary>
	///     "Global" configuration of the log viewer.
	///     Maintains settings which are not changeable per data source, but once
	///     for the entire application.
	/// </summary>
	public interface ILogViewerSettings
	{
		/// <summary>
		/// </summary>
		int LinesScrolledPerWheelTick { get; set; }

		/// <summary>
		/// </summary>
		int FontSize { get; set; }

		/// <summary>
		///     The width of a tab-character expressed in number of spaces.
		/// </summary>
		int TabWidth { get; set; }
		
		/// <summary>
		///    The settings concerning trace-level log entries.
		/// </summary>
		LogLevelSettings Trace { get; }

		/// <summary>
		///    The settings concerning debug-level log entries.
		/// </summary>
		LogLevelSettings Debug { get; }

		/// <summary>
		///    The settings concerning info-level log entries.
		/// </summary>
		LogLevelSettings Info { get; }

		/// <summary>
		///    The settings concerning warning-level log entries.
		/// </summary>
		LogLevelSettings Warning { get; }

		/// <summary>
		///    The settings concerning error-level log entries.
		/// </summary>
		LogLevelSettings Error { get; }

		/// <summary>
		///    The settings concerning fatal-level log entries.
		/// </summary>
		LogLevelSettings Fatal { get; }
	}
}