using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class AbstractReadOnlyLogEntry
		: IReadOnlyLogEntry
	{
		/// <inheritdoc />
		public string RawContent => GetValue(LogFileColumns.RawContent);

		/// <inheritdoc />
		public LogLineIndex Index => GetValue(LogFileColumns.Index);

		/// <inheritdoc />
		public LogLineIndex OriginalIndex => GetValue(LogFileColumns.OriginalIndex);

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex => GetValue(LogFileColumns.LogEntryIndex);

		/// <inheritdoc />
		public int LineNumber => GetValue(LogFileColumns.LineNumber);

		/// <inheritdoc />
		public int OriginalLineNumber => GetValue(LogFileColumns.OriginalLineNumber);

		/// <inheritdoc />
		public string OriginalDataSourceName => GetValue(LogFileColumns.OriginalDataSourceName);

		/// <inheritdoc />
		public LogLineSourceId SourceId => GetValue(LogFileColumns.SourceId);

		/// <inheritdoc />
		public LevelFlags LogLevel => GetValue(LogFileColumns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetValue(LogFileColumns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetValue(LogFileColumns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetValue(LogFileColumns.DeltaTime);

		/// <inheritdoc />
		public abstract T GetValue<T>(ILogFileColumnDescriptor<T> column);

		/// <inheritdoc />
		public abstract bool TryGetValue<T>(ILogFileColumnDescriptor<T> column, out T value);

		/// <inheritdoc />
		public abstract object GetValue(ILogFileColumnDescriptor column);

		/// <inheritdoc />
		public abstract bool TryGetValue(ILogFileColumnDescriptor column, out object value);

		/// <inheritdoc />
		public abstract IReadOnlyList<ILogFileColumnDescriptor> Columns { get; }

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return ReadOnlyLogEntryExtensions.Equals(this, obj as IReadOnlyLogEntry);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			// TODO: What should we do here?
			return base.GetHashCode();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return ReadOnlyLogEntryExtensions.ToString(this);
		}
	}
}