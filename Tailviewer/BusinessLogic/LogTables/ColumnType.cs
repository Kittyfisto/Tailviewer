namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     Describes how the data of a column is to be interpreted, if known.
	/// </summary>
	public enum ColumnType
	{
		/// <summary>
		/// The data contained in this column can't be interpreted.
		/// </summary>
		Unknown = 0,

		ThreadName,
		Timestamp,
		Message,
		Level,

		/// <summary>
		/// The column contains the name of the logger.
		/// </summary>
		Logger,
	}
}