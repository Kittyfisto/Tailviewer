using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// A registry of all log file formats known by Tailviewer.
	/// </summary>
	public interface ILogFileFormatRegistry
	{
		/// <summary>
		/// Returns the current list of supported log file formats.
		/// </summary>
		IReadOnlyList<ILogFileFormat> Formats { get; }
	}
}
