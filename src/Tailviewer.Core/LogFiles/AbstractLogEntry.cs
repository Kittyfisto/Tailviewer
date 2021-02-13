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
			get { return GetValue(LogFileColumns.RawContent); }
			set { SetValue(LogFileColumns.RawContent, value); }
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get { return GetValue(LogFileColumns.Index); }
			set { SetValue(LogFileColumns.Index, value); }
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get { return GetValue(LogFileColumns.OriginalIndex); }
			set { SetValue(LogFileColumns.OriginalIndex, value); }
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get { return GetValue(LogFileColumns.LogEntryIndex); }
			set { SetValue(LogFileColumns.LogEntryIndex, value); }
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get { return GetValue(LogFileColumns.LineNumber); }
			set { SetValue(LogFileColumns.LineNumber, value); }
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get { return GetValue(LogFileColumns.OriginalLineNumber); }
			set { SetValue(LogFileColumns.OriginalLineNumber, value); }
		}

		/// <inheritdoc />
		public string OriginalDataSourceName
		{
			get { return GetValue(LogFileColumns.OriginalDataSourceName); }
			set { SetValue(LogFileColumns.OriginalDataSourceName, value); }
		}

		/// <inheritdoc />
		public LogLineSourceId SourceId
		{
			get { return GetValue(LogFileColumns.SourceId); }
			set { SetValue(LogFileColumns.SourceId, value); }
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get { return GetValue(LogFileColumns.LogLevel); }
			set { SetValue(LogFileColumns.LogLevel, value); }
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get { return GetValue(LogFileColumns.Timestamp); }
			set { SetValue(LogFileColumns.Timestamp, value); }
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get { return GetValue(LogFileColumns.ElapsedTime); }
			set { SetValue(LogFileColumns.ElapsedTime, value); }
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get { return GetValue(LogFileColumns.DeltaTime); }
			set { SetValue(LogFileColumns.DeltaTime, value); }
		}

		/// <inheritdoc />
		public abstract T GetValue<T>(ILogFileColumnDescriptor<T> column);

		/// <inheritdoc />
		public abstract bool TryGetValue<T>(ILogFileColumnDescriptor<T> column, out T value);

		/// <inheritdoc />
		public abstract object GetValue(ILogFileColumnDescriptor column);

		/// <inheritdoc />
		public abstract bool TryGetValue(ILogFileColumnDescriptor column, out object value);

		/// <inheritdoc />
		public abstract void SetValue(ILogFileColumnDescriptor column, object value);

		/// <inheritdoc />
		public abstract void SetValue<T>(ILogFileColumnDescriptor<T> column, T value);

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