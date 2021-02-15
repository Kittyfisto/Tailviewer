using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles.Text
{
	/// <summary>
	///     Simple <see cref="IReadOnlyLogEntry" /> implementation for unparsed log lines.
	/// </summary>
	internal sealed class RawTextLogEntry
		: IReadOnlyLogEntry
	{
		private static readonly IReadOnlyList<IColumnDescriptor> AllColumns = new IColumnDescriptor[]
		{
			LogFiles.Columns.RawContent,
			LogFiles.Columns.Index,
			LogFiles.Columns.OriginalIndex,
			LogFiles.Columns.LineNumber,
			LogFiles.Columns.OriginalLineNumber,
			LogFiles.Columns.OriginalDataSourceName,
			LogFiles.Columns.LogEntryIndex
		};

		private readonly LogLineIndex _index;
		private readonly string _rawContent;
		private readonly string _fullFilename;

		public RawTextLogEntry(LogLineIndex index,
		                       string rawContent,
		                       string fullFilename)
		{
			_index = index;
			_rawContent = rawContent;
			_fullFilename = fullFilename;
		}

		#region Implementation of IReadOnlyLogEntry

		public string RawContent
		{
			get { return _rawContent; }
		}

		public LogLineIndex Index
		{
			get { return _index; }
		}

		public LogLineIndex OriginalIndex
		{
			get { return _index; }
		}

		public LogEntryIndex LogEntryIndex
		{
			get { return (int)_index; }
		}

		public int LineNumber
		{
			get { return _index.Value + 1; }
		}

		public int OriginalLineNumber
		{
			get { return LineNumber; }
		}

		public string OriginalDataSourceName
		{
			get
			{
				return _fullFilename;
			}
		}

		public LogLineSourceId SourceId
		{
			get { throw new NotImplementedException(); }
		}

		public LevelFlags LogLevel => LevelFlags.None;

		public DateTime? Timestamp => null;

		public TimeSpan? ElapsedTime => null;

		public TimeSpan? DeltaTime => null;

		public T GetValue<T>(IColumnDescriptor<T> column)
		{
			if (!TryGetValue(column, out var value))
				throw new NoSuchColumnException(column);

			return value;
		}

		public bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
		{
			if (TryGetValue(column, out object tmp))
			{
				value = (T) tmp;
				return true;
			}

			value = default;
			return false;
		}

		public object GetValue(IColumnDescriptor column)
		{
			if (!TryGetValue(column, out var value))
				throw new NoSuchColumnException(column);

			return value;
		}

		public bool TryGetValue(IColumnDescriptor column, out object value)
		{
			if (Equals(column, LogFiles.Columns.RawContent))
			{
				value = RawContent;
				return true;
			}
			if (Equals(column, LogFiles.Columns.Index) || Equals(column, LogFiles.Columns.OriginalIndex))
			{
				value = Index;
				return true;
			}
			if (Equals(column, LogFiles.Columns.LineNumber) || Equals(column, LogFiles.Columns.OriginalLineNumber))
			{
				value = LineNumber;
				return true;
			}
			if (Equals(column, LogFiles.Columns.LogEntryIndex))
			{
				value = LogEntryIndex;
				return true;
			}
			if (Equals(column, LogFiles.Columns.OriginalDataSourceName))
			{
				value = OriginalDataSourceName;
				return true;
			}

			value = default;
			return false;
		}

		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get { return AllColumns; }
		}

		#endregion
	}
}