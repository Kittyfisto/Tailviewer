using System;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     This <see cref="ILogTable" /> implementation offers a tabular view onto a <see cref="ILogFile" />
	///     by interpreting each log entry using a given pattern, such as the ones used by log4net.
	/// </summary>
	public sealed class LogFileTable
		: ILogTable
	{
		public int Count
		{
			get { throw new NotImplementedException(); }
		}

		public bool Exists
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LastModified
		{
			get { throw new NotImplementedException(); }
		}

		public ILogTableSchema Schema
		{
			get { throw new NotImplementedException(); }
		}

		public ITask<LogEntry> this[LogEntryIndex index]
		{
			get { throw new NotImplementedException(); }
		}

		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			throw new NotImplementedException();
		}

		public bool RemoveListener(ILogTableListener listener)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}