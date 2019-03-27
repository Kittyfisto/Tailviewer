using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     The various error flags a log file may set to indicate as to why a data source doesn't
	///     display any data.
	/// </summary>
	/// <remarks>
	///     TODO: Rename to EmptyReason
	/// </remarks>
	[Flags]
	public enum ErrorFlags : uint
	{
		/// <summary>
		///     There is no error. If no data is shown, then the data source is empty.
		/// </summary>
		None = 0,

		/// <summary>
		///     The data source (represented by the <see cref="ILogFile" />) doesn't exist.
		/// </summary>
		/// <remarks>
		///     Examples of when this flag should be set:
		///     - The data source represents a table from a SQL server and the connection was interrupted
		///     - The data source represents a file on disk and that has just been deleted
		/// </remarks>
		SourceDoesNotExist = 0x01,

		/// <summary>
		///     The data source exists, but cannot be accessed.
		/// </summary>
		/// <remarks>
		///     Examples of when this flag should be set:
		///     - The user isn't authorized to view the file
		///     - The file is opened in exclusive mode by another application
		/// </remarks>
		SourceCannotBeAccessed = 0x02
	}
}