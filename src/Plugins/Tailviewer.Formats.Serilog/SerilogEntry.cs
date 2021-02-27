using System;
using System.Collections.Generic;
using Tailviewer.Core.Columns;

namespace Tailviewer.Formats.Serilog
{
	public sealed class SerilogEntry
		: IReadOnlyLogEntry
	{
		private readonly string _rawContent;
		public LevelFlags LogLevel;
		public DateTime Timestamp;

		public SerilogEntry(string rawContent)
		{
			_rawContent = rawContent;
		}

		#region Implementation of IReadOnlyLogEntry

		public string RawContent
		{
			get
			{
				return _rawContent;
			}
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

		public string OriginalDataSourceName
		{
			get { throw new NotImplementedException(); }
		}

		public LogEntrySourceId SourceId
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

		public string Message
		{
			get;
			set;
		}

		public T GetValue<T>(IColumnDescriptor<T> column)
		{
			if (Equals(column, GeneralColumns.RawContent))
			{
				return (T) (object) _rawContent;
			}

			if (Equals(column, GeneralColumns.Message))
			{
				return (T)(object)Message;
			}

			throw new NotImplementedException();
		}

		public bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
		{
			throw new NotImplementedException();
		}

		public object GetValue(IColumnDescriptor column)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(IColumnDescriptor column, out object value)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get { throw new NotImplementedException(); }
		}

		public bool Contains(IColumnDescriptor column)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}