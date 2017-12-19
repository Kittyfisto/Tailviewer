namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Represents a log entry which can be modified.
	/// </summary>
	public interface ILogEntry
		: IReadOnlyLogEntry
	{
		/// <summary>
		///     Sets the value of this log entry for the given column.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		void SetColumnValue(ILogFileColumn column, object value);

		/// <summary>
		///     Sets the value of this log entry for the given column.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		void SetColumnValue<T>(ILogFileColumn<T> column, T value);
	}
}