using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
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
			get { return GetValue(Core.Columns.RawContent); }
			set { SetValue(Core.Columns.RawContent, value); }
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get { return GetValue(Core.Columns.Index); }
			set { SetValue(Core.Columns.Index, value); }
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get { return GetValue(Core.Columns.OriginalIndex); }
			set { SetValue(Core.Columns.OriginalIndex, value); }
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get { return GetValue(Core.Columns.LogEntryIndex); }
			set { SetValue(Core.Columns.LogEntryIndex, value); }
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get { return GetValue(Core.Columns.LineNumber); }
			set { SetValue(Core.Columns.LineNumber, value); }
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get { return GetValue(Core.Columns.OriginalLineNumber); }
			set { SetValue(Core.Columns.OriginalLineNumber, value); }
		}

		/// <inheritdoc />
		public string OriginalDataSourceName
		{
			get { return GetValue(Core.Columns.OriginalDataSourceName); }
			set { SetValue(Core.Columns.OriginalDataSourceName, value); }
		}

		/// <inheritdoc />
		public LogEntrySourceId SourceId
		{
			get { return GetValue(Core.Columns.SourceId); }
			set { SetValue(Core.Columns.SourceId, value); }
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get { return GetValue(Core.Columns.LogLevel); }
			set { SetValue(Core.Columns.LogLevel, value); }
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get { return GetValue(Core.Columns.Timestamp); }
			set { SetValue(Core.Columns.Timestamp, value); }
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get { return GetValue(Core.Columns.ElapsedTime); }
			set { SetValue(Core.Columns.ElapsedTime, value); }
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get { return GetValue(Core.Columns.DeltaTime); }
			set { SetValue(Core.Columns.DeltaTime, value); }
		}

		/// <inheritdoc />
		public string Message
		{
			get { return GetValue(Core.Columns.Message); }
			set { SetValue(Core.Columns.Message, value); }
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