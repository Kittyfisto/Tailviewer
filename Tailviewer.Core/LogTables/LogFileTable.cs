using System;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogTables
{
	/// <summary>
	///     This <see cref="ILogTable" /> implementation offers a tabular view onto a <see cref="ILogFile" />
	///     by interpreting each log entry using a given pattern, such as the ones used by log4net.
	/// </summary>
	public sealed class LogFileTable
		: ILogTable
	{
		/// <inheritdoc />
		public int Count
		{
			get { throw new NotImplementedException(); }
		}

		/// <inheritdoc />
		public bool Exists
		{
			get { throw new NotImplementedException(); }
		}

		/// <inheritdoc />
		public DateTime LastModified
		{
			get { throw new NotImplementedException(); }
		}

		/// <inheritdoc />
		public ILogTableSchema Schema
		{
			get { throw new NotImplementedException(); }
		}

		/// <inheritdoc />
		public ITask<LogEntry> this[LogEntryIndex index]
		{
			get { throw new NotImplementedException(); }
		}

		/// <inheritdoc />
		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public bool RemoveListener(ILogTableListener listener)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}