using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     A registry of all log file formats known by Tailviewer. Allows users to query the list of currently known
	///     (and supported) log file formats.
	/// </summary>
	[Service]
	public interface ILogFileFormatRepository
	{
		/// <summary>
		///     Returns the current list of supported log file formats.
		/// </summary>
		IReadOnlyList<ILogFileFormat> Formats { get; }
	}
}