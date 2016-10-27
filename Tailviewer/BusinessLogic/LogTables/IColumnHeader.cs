namespace Tailviewer.BusinessLogic.LogTables
{
	public interface IColumnHeader
	{
		/// <summary>
		///     The name of this column, is displayed to the user verbatim.
		/// </summary>
		string Name { get; }
	}
}