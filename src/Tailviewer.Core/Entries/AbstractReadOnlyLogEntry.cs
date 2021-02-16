using System;
using System.Collections.Generic;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Entries
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class AbstractReadOnlyLogEntry
		: IReadOnlyLogEntry
	{
		/// <inheritdoc />
		public string RawContent => GetValue(LogColumns.RawContent);

		/// <inheritdoc />
		public LogLineIndex Index => GetValue(LogColumns.Index);

		/// <inheritdoc />
		public LogLineIndex OriginalIndex => GetValue(LogColumns.OriginalIndex);

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex => GetValue(LogColumns.LogEntryIndex);

		/// <inheritdoc />
		public int LineNumber => GetValue(LogColumns.LineNumber);

		/// <inheritdoc />
		public int OriginalLineNumber => GetValue(LogColumns.OriginalLineNumber);

		/// <inheritdoc />
		public string OriginalDataSourceName => GetValue(LogColumns.OriginalDataSourceName);

		/// <inheritdoc />
		public LogLineSourceId SourceId => GetValue(LogColumns.SourceId);

		/// <inheritdoc />
		public LevelFlags LogLevel => GetValue(LogColumns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetValue(LogColumns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetValue(LogColumns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetValue(LogColumns.DeltaTime);

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