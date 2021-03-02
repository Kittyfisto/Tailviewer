using Tailviewer.Api;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Responsible for presenting one column of a log file.
	/// </summary>
	public interface ILogFileColumnPresenter
	{
		IColumnDescriptor Column { get; }

		TextSettings TextSettings { get; set; }

		/// <summary>
		///     Fetches the newest values for this presenter's column from the given log file.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="visibleSection"></param>
		/// <param name="yOffset"></param>
		void FetchValues(ILogSource logSource, LogSourceSection visibleSection, double yOffset);
	}
}