namespace Tailviewer.BusinessLogic.LogTables
{
	public interface IColumnHeader
	{
		/// <summary>
		///     The name of this column, is displayed to the user verbatim.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The maximum number of characters in this column.
		/// </summary>
		int MaximumLength { get; }
	}
}