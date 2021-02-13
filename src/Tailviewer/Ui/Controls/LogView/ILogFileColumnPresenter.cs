using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Responsible for presenting one column of a log file.
	/// </summary>
	public interface ILogFileColumnPresenter
	{
		ILogFileColumnDescriptor Column { get; }

		TextSettings TextSettings { get; set; }

		/// <summary>
		///     Fetches the newest values for this presenter's column from the given log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="visibleSection"></param>
		/// <param name="yOffset"></param>
		void FetchValues(ILogFile logFile, LogFileSection visibleSection, double yOffset);
	}
}