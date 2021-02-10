using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     The interface to store data of a specific column for a <see cref="LogEntryList"/>.
	/// </summary>
	public interface IColumnData
	{
		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		object this[int index] { get; }

		/// <summary>
		/// </summary>
		void Clear();

		/// <summary>
		/// </summary>
		/// <param name="logEntry"></param>
		void Add(IReadOnlyLogEntry logEntry);

		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		void RemoveAt(int index);

		/// <summary>
		/// </summary>
		void AddEmpty();

		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="logEntry"></param>
		void Insert(int index, IReadOnlyLogEntry logEntry);

		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		void RemoveRange(int index, int count);

		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		void InsertEmpty(int index);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indices"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex"></param>
		void CopyTo(IReadOnlyList<int> indices, ILogEntries buffer, int destinationIndex);
	}
}