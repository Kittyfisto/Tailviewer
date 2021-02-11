using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read-only access to a list of log entries.
	/// </summary>
	/// <remarks>
	///     The order/nature of these entries is undefined as well as their source.
	/// </remarks>
	public interface IReadOnlyLogEntries
		: IReadOnlyList<IReadOnlyLogEntry>
	{
		/// <summary>
		///     The list of columns to store in this buffer.
		/// </summary>
		IReadOnlyList<ILogFileColumn> Columns { get; }

		/// <summary>
		///     Tests if this buffer contains the given column.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		bool Contains(ILogFileColumn column);

		/// <summary>
		///     Copies values from the given <paramref name="column" /> into the given <paramref name="destination" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <param name="sourceIndex"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="length"></param>
		/// <exception cref="NoSuchColumnException"></exception>
		void CopyTo<T>(ILogFileColumn<T> column, int sourceIndex, T[] destination, int destinationIndex, int length);

		/// <summary>
		///     Copies values from the given <paramref name="column" /> into the given <paramref name="destination" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		/// <exception cref="NoSuchColumnException"></exception>
		void CopyTo<T>(ILogFileColumn<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex);
	}
}