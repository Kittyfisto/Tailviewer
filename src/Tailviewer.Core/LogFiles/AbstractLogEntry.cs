using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
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
			get { return GetValue(LogFiles.Columns.RawContent); }
			set { SetValue(LogFiles.Columns.RawContent, value); }
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get { return GetValue(LogFiles.Columns.Index); }
			set { SetValue(LogFiles.Columns.Index, value); }
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get { return GetValue(LogFiles.Columns.OriginalIndex); }
			set { SetValue(LogFiles.Columns.OriginalIndex, value); }
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get { return GetValue(LogFiles.Columns.LogEntryIndex); }
			set { SetValue(LogFiles.Columns.LogEntryIndex, value); }
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get { return GetValue(LogFiles.Columns.LineNumber); }
			set { SetValue(LogFiles.Columns.LineNumber, value); }
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get { return GetValue(LogFiles.Columns.OriginalLineNumber); }
			set { SetValue(LogFiles.Columns.OriginalLineNumber, value); }
		}

		/// <inheritdoc />
		public string OriginalDataSourceName
		{
			get { return GetValue(LogFiles.Columns.OriginalDataSourceName); }
			set { SetValue(LogFiles.Columns.OriginalDataSourceName, value); }
		}

		/// <inheritdoc />
		public LogLineSourceId SourceId
		{
			get { return GetValue(LogFiles.Columns.SourceId); }
			set { SetValue(LogFiles.Columns.SourceId, value); }
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get { return GetValue(LogFiles.Columns.LogLevel); }
			set { SetValue(LogFiles.Columns.LogLevel, value); }
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get { return GetValue(LogFiles.Columns.Timestamp); }
			set { SetValue(LogFiles.Columns.Timestamp, value); }
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get { return GetValue(LogFiles.Columns.ElapsedTime); }
			set { SetValue(LogFiles.Columns.ElapsedTime, value); }
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get { return GetValue(LogFiles.Columns.DeltaTime); }
			set { SetValue(LogFiles.Columns.DeltaTime, value); }
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