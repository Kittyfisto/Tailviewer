using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     Responsible for extracting the data of a particular column from one or more <see cref="LogLine" />(s).
	/// </summary>
	public interface IColumnParser
	{
		/// <summary>
		///     The name of this column.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     How the column's data is to be interpreted.
		/// </summary>
		ColumnType Type { get; }
	}
}