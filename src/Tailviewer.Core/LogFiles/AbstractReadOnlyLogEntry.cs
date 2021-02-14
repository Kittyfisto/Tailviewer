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
		public string RawContent => GetValue(LogFiles.Columns.RawContent);

		/// <inheritdoc />
		public LogLineIndex Index => GetValue(LogFiles.Columns.Index);

		/// <inheritdoc />
		public LogLineIndex OriginalIndex => GetValue(LogFiles.Columns.OriginalIndex);

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex => GetValue(LogFiles.Columns.LogEntryIndex);

		/// <inheritdoc />
		public int LineNumber => GetValue(LogFiles.Columns.LineNumber);

		/// <inheritdoc />
		public int OriginalLineNumber => GetValue(LogFiles.Columns.OriginalLineNumber);

		/// <inheritdoc />
		public string OriginalDataSourceName => GetValue(LogFiles.Columns.OriginalDataSourceName);

		/// <inheritdoc />
		public LogLineSourceId SourceId => GetValue(LogFiles.Columns.SourceId);

		/// <inheritdoc />
		public LevelFlags LogLevel => GetValue(LogFiles.Columns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetValue(LogFiles.Columns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetValue(LogFiles.Columns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetValue(LogFiles.Columns.DeltaTime);

		/// <inheritdoc />
		public abstract T GetValue<T>(IColumnDescriptor<T> column);

		/// <inheritdoc />
		public abstract bool TryGetValue<T>(IColumnDescriptor<T> column, out T value);

		/// <inheritdoc />
		public abstract object GetValue(IColumnDescriptor column);

		/// <inheritdoc />
		public abstract bool TryGetValue(IColumnDescriptor column, out object value);

		/// <inheritdoc />
		public abstract IReadOnlyList<IColumnDescriptor> Columns { get; }

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