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
		public LevelFlags LogLevel => GetValue(LogFileColumns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetValue(LogFileColumns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetValue(LogFileColumns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetValue(LogFileColumns.DeltaTime);

		/// <inheritdoc />
		public abstract T GetValue<T>(ILogFileColumn<T> column);

		/// <inheritdoc />
		public abstract bool TryGetValue<T>(ILogFileColumn<T> column, out T value);

		/// <inheritdoc />
		public abstract object GetValue(ILogFileColumn column);

		/// <inheritdoc />
		public abstract bool TryGetValue(ILogFileColumn column, out object value);

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