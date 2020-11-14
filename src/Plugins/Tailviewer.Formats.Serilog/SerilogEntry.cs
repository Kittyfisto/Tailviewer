using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Formats.Serilog
{
	public sealed class SerilogEntry
		: IReadOnlyLogEntry
	{
		public LevelFlags LogLevel;
		public DateTime Timestamp;
		public string Message;

		#region Implementation of IReadOnlyLogEntry

		public string RawContent
		{
			get { throw new NotImplementedException(); }
		}

		public LogLineIndex Index
		{
			get { throw new NotImplementedException(); }
		}

		public LogLineIndex OriginalIndex
		{
			get { throw new NotImplementedException(); }
		}

		public LogEntryIndex LogEntryIndex
		{
			get { throw new NotImplementedException(); }
		}

		public int LineNumber
		{
			get { throw new NotImplementedException(); }
		}

		public int OriginalLineNumber
		{
			get { throw new NotImplementedException(); }
		}

		LevelFlags IReadOnlyLogEntry.LogLevel
		{
			get { return LogLevel; }
		}

		DateTime? IReadOnlyLogEntry.Timestamp
		{
			get { return Timestamp; }
		}

		public TimeSpan? ElapsedTime
		{
			get { throw new NotImplementedException(); }
		}

		public TimeSpan? DeltaTime
		{
			get { throw new NotImplementedException(); }
		}

		public T GetValue<T>(ILogFileColumn<T> column)
		{
			if (Equals(column, LogFileColumns.Message))
			{
				return (T)(object)Message;
			}

			throw new NotImplementedException();
		}

		public bool TryGetValue<T>(ILogFileColumn<T> column, out T value)
		{
			throw new NotImplementedException();
		}

		public object GetValue(ILogFileColumn column)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(ILogFileColumn column, out object value)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<ILogFileColumn> Columns
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}