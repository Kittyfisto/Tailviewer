using System;
using System.Collections.Generic;
using System.Text;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
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
		public abstract T GetValue<T>(ILogFileColumn<T> column);

		/// <inheritdoc />
		public abstract bool TryGetValue<T>(ILogFileColumn<T> column, out T value);

		/// <inheritdoc />
		public abstract object GetValue(ILogFileColumn column);

		/// <inheritdoc />
		public abstract bool TryGetValue(ILogFileColumn column, out object value);

		/// <inheritdoc />
		public abstract void SetValue(ILogFileColumn column, object value);

		/// <inheritdoc />
		public abstract void SetValue<T>(ILogFileColumn<T> column, T value);

		/// <inheritdoc />
		public abstract IReadOnlyList<ILogFileColumn> Columns { get; }

		/// <inheritdoc />
		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			foreach (var column in Columns)
			{
				if (stringBuilder.Length > 0)
					stringBuilder.Append("|");
				stringBuilder.Append(GetValue(column));
			}
			return stringBuilder.ToString();
		}
	}
}