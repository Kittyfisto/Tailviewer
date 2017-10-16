namespace Tailviewer.Core.LogTables
{
	/// <summary>
	///     The header for a column.
	/// </summary>
	public interface IColumnHeader
	{
		/// <summary>
		///     The name of this column, is displayed to the user verbatim.
		/// </summary>
		string Name { get; }
	}
}