using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer
{
	/// <summary>
	///     Provides read-only access to a collection of log entries stored in memory.
	/// </summary>
	/// <remarks>
	///     Buffers behave like lists / arrays in that log entries are stored in a particular order, referring
	///     to the order they were added to / copied into the buffer.
	///     This aspect drastically differs from <see cref="ILogSource"/>s which behave more like dictionaries,
	///     providing access to log entries by their <see cref="LogLineIndex"/> instead.
	/// </remarks>
	/// <remarks>
	///     Is mostly used to retrieve a portion of log entries from <see cref="ILogSource"/>s.
	/// </remarks>
	public interface IReadOnlyLogBuffer
		: IReadOnlyList<IReadOnlyLogEntry>
	{
		/// <summary>
		///     The list of columns to store in this buffer.
		/// </summary>
		IReadOnlyList<IColumnDescriptor> Columns { get; }

		/// <summary>
		///     Tests if this buffer contains the given column.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		bool Contains(IColumnDescriptor column);

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
		void CopyTo<T>(IColumnDescriptor<T> column, int sourceIndex, T[] destination, int destinationIndex, int length);

		/// <summary>
		///     Copies values from the given <paramref name="column" /> into the given <paramref name="destination" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		/// <exception cref="NoSuchColumnException"></exception>
		void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex);

		/// <summary>
		///     Copies values from the given <paramref name="column" /> into the given <paramref name="destination" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		/// <exception cref="NoSuchColumnException"></exception>
		void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, IList<T> destination, int destinationIndex);
	}
}