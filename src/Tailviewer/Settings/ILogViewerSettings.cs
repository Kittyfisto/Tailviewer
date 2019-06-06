
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
	}
}