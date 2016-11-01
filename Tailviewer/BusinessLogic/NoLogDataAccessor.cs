namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     A <see cref="ILogDataAccessoror{TIndex,TData}" /> implementation that doesn't allow any access
	///     to the data. Can be used when it's already known that the data source doesn't exist.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public sealed class NoLogDataAccessor<TIndex, TData>
		: ILogDataAccessor<TIndex, TData>
	{
		public bool TryAccess(TIndex index, out TData data)
		{
			data = default(TData);
			return false;
		}
	}
}