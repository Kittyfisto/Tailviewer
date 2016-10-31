namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     Is called whenever the <see cref="ILogTable" /> it has been added to is modified.
	/// </summary>
	public interface ILogTableListener
	{
		/// <summary>
		/// Is called whenever the given <paramref name="logTable"/> is modified.
		/// A modification is:
		/// - one or more entries have been added
		/// - one or more entries have been invalidated
		/// - the entire table has been reset
		/// - the columns changed
		/// </summary>
		/// <param name="logTable"></param>
		/// <param name="modification"></param>
		void OnLogTableModified(ILogTable logTable, LogTableModification modification);
	}
}