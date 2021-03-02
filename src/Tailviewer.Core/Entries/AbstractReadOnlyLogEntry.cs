using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;
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
		public string RawContent => GetValue(GeneralColumns.RawContent);

		/// <inheritdoc />
		public LogLineIndex Index => GetValue(GeneralColumns.Index);

		/// <inheritdoc />
		public LogLineIndex OriginalIndex => GetValue(GeneralColumns.OriginalIndex);

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex => GetValue(GeneralColumns.LogEntryIndex);

		/// <inheritdoc />
		public int LineNumber => GetValue(GeneralColumns.LineNumber);

		/// <inheritdoc />
		public int OriginalLineNumber => GetValue(GeneralColumns.OriginalLineNumber);

		/// <inheritdoc />
		public string OriginalDataSourceName => GetValue(GeneralColumns.OriginalDataSourceName);

		/// <inheritdoc />
		public LogEntrySourceId SourceId => GetValue(GeneralColumns.SourceId);

		/// <inheritdoc />
		public LevelFlags LogLevel => GetValue(GeneralColumns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetValue(GeneralColumns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetValue(GeneralColumns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetValue(GeneralColumns.DeltaTime);

		/// <inheritdoc />
		public string Message => GetValue(GeneralColumns.Message);

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
		public bool Contains(IColumnDescriptor column)
		{
			return Columns.Contains(column);
		}

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