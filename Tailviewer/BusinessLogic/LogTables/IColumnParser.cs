using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     Responsible for extracting the data of a particular column from one or more <see cref="LogLine" />(s).
	/// </summary>
	public interface IColumnParser
	{
		/// <summary>
		///     The pattern of this column.
		/// </summary>
		string Pattern { get; }

		/// <summary>
		///     The name of this column.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     How the column's data is to be interpreted.
		/// </summary>
		ColumnType Type { get; }

		/// <summary>
		///     Parses a portion of the given line and returns the parsed data.
		/// </summary>
		/// <param name="line"></param>
		/// <param name="startIndex"></param>
		/// <param name="numCharactersConsumed">The number of characters that were consumed from line from 'startIndex' onward</param>
		/// <returns></returns>
		object Parse(string line, int startIndex, out int numCharactersConsumed);
	}
}