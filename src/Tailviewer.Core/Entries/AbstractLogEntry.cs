using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Entries
{
	/// <summary>
	///    An abstract implementation of <see cref="ILogEntry"/>.
	///    Delegates all fixed property access calls to <see cref="GetValue{T}"/> and <see cref="SetValue"/>.
	/// </summary>
	/// <remarks>
	///    IF plugin authors desperately need to implement their own <see cref="ILogEntry"/>, then they should
	///    inherit from this class instead. They don't have to implement all properties and are protected against
	///    having to re-compile against future additions.
	/// </remarks>
	public abstract class AbstractLogEntry
		: ILogEntry
	{
		/// <inheritdoc />
		public string RawContent
		{
			get { return GetValue(LogColumns.RawContent); }
			set { SetValue(LogColumns.RawContent, value); }
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get { return GetValue(LogColumns.Index); }
			set { SetValue(LogColumns.Index, value); }
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get { return GetValue(LogColumns.OriginalIndex); }
			set { SetValue(LogColumns.OriginalIndex, value); }
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get { return GetValue(LogColumns.LogEntryIndex); }
			set { SetValue(LogColumns.LogEntryIndex, value); }
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get { return GetValue(LogColumns.LineNumber); }
			set { SetValue(LogColumns.LineNumber, value); }
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get { return GetValue(LogColumns.OriginalLineNumber); }
			set { SetValue(LogColumns.OriginalLineNumber, value); }
		}

		/// <inheritdoc />
		public string OriginalDataSourceName
		{
			get { return GetValue(LogColumns.OriginalDataSourceName); }
			set { SetValue(LogColumns.OriginalDataSourceName, value); }
		}

		/// <inheritdoc />
		public LogLineSourceId SourceId
		{
			get { return GetValue(LogColumns.SourceId); }
			set { SetValue(LogColumns.SourceId, value); }
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get { return GetValue(LogColumns.LogLevel); }
			set { SetValue(LogColumns.LogLevel, value); }
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get { return GetValue(LogColumns.Timestamp); }
			set { SetValue(LogColumns.Timestamp, value); }
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get { return GetValue(LogColumns.ElapsedTime); }
			set { SetValue(LogColumns.ElapsedTime, value); }
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get { return GetValue(LogColumns.DeltaTime); }
			set { SetValue(LogColumns.DeltaTime, value); }
		}

		/// <inheritdoc />
		public abstract T GetValue<T>(IColumnDescriptor<T> column);

		/// <inheritdoc />
		public abstract bool TryGetValue<T>(IColumnDescriptor<T> column, out T value);

		/// <inheritdoc />
		public abstract object GetValue(IColumnDescriptor column);

		/// <inheritdoc />
		public abstract bool TryGetValue(IColumnDescriptor column, out object value);

		/// <inheritdoc />
		public abstract void SetValue(IColumnDescriptor column, object value);

		/// <inheritdoc />
		public abstract void SetValue<T>(IColumnDescriptor<T> column, T value);

		/// <inheritdoc />
		public abstract IReadOnlyList<IColumnDescriptor> Columns { get; }

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return ReadOnlyLogEntryExtensions.Equals(this, obj as IReadOnlyLogEntry);
		}

		/// <inheritdoc />
		public virtual void CopyFrom(IReadOnlyLogEntry logEntry)
		{
			var overlappingColumns = logEntry.Columns.Intersect(Columns);
			foreach (var column in overlappingColumns)
			{
				SetValue(column, logEntry.GetValue(column));
			}
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