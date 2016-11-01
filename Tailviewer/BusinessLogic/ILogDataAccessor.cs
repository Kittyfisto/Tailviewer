using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     An interface that allows synchronous access to a portion of a data source.
	///     Is meant to be implemented and used by <see cref="ILogFile" /> and <see cref="ILogTable" />
	///     implementations in conjunction with <see cref="LogDataAccessQueue{TIndex, TData}" />.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public interface ILogDataAccessor<in TIndex, TData>
	{
		/// <summary>
		///     Tries to access the data at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		/// <returns>true when the data could be accessed, false when the index is invalid</returns>
		bool TryAccess(TIndex index, out TData data);
	}
}