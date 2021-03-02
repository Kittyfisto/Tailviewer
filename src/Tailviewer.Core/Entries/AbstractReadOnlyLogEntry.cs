using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class AbstractReadOnlyLogEntry
		: IReadOnlyLogEntry
	{
		/// <inheritdoc />
		public string RawContent => GetValue(Core.Columns.RawContent);

		/// <inheritdoc />
		public LogLineIndex Index => GetValue(Core.Columns.Index);

		/// <inheritdoc />
		public LogLineIndex OriginalIndex => GetValue(Core.Columns.OriginalIndex);

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex => GetValue(Core.Columns.LogEntryIndex);

		/// <inheritdoc />
		public int LineNumber => GetValue(Core.Columns.LineNumber);

		/// <inheritdoc />
		public int OriginalLineNumber => GetValue(Core.Columns.OriginalLineNumber);

		/// <inheritdoc />
		public string OriginalDataSourceName => GetValue(Core.Columns.OriginalDataSourceName);

		/// <inheritdoc />
		public LogEntrySourceId SourceId => GetValue(Core.Columns.SourceId);

		/// <inheritdoc />
		public LevelFlags LogLevel => GetValue(Core.Columns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetValue(Core.Columns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetValue(Core.Columns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetValue(Core.Columns.DeltaTime);

		/// <inheritdoc />
		public string Message => GetValue(Core.Columns.Message);

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