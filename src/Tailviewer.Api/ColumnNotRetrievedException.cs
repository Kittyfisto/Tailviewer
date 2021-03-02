namespace Tailviewer.Api
{
	/// <summary>
	///     This exception is thrown when a particular column MIGHT exist, but it hasn't been queried
	///     from the source <see cref="ILogSource" /> and therefore isn't present.
	/// </summary>
	public sealed class ColumnNotRetrievedException
		: NoSuchColumnException
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="column"></param>
		public ColumnNotRetrievedException(IColumnDescriptor column)
			: base(string.Format("No column with the id '{0}' has been retrieved. You should fetch it if you think it exists!", column.Id))
		{
		}
	}
}