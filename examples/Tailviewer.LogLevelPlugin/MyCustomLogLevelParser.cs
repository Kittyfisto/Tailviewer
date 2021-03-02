using System;
using System.Collections.Generic;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.LogLevelPlugin
{
	public sealed class MyCustomLogLevelParser
		: ILogEntryParser
	{
		#region Implementation of IDisposable

		public void Dispose()
		{
		}

		#endregion

		private LevelFlags GetLogLevel(string lineMessage)
		{
			if (lineMessage.IndexOf("FAT", StringComparison.CurrentCulture) != -1)
				return LevelFlags.Fatal;
			if (lineMessage.IndexOf("ERR", StringComparison.CurrentCulture) != -1)
				return LevelFlags.Error;
			if (lineMessage.IndexOf("WARN", StringComparison.CurrentCulture) != -1)
				return LevelFlags.Warning;
			if (lineMessage.IndexOf("INF", StringComparison.CurrentCulture) != -1)
				return LevelFlags.Info;
			if (lineMessage.IndexOf("DBG", StringComparison.CurrentCulture) != -1)
				return LevelFlags.Debug;
			if (lineMessage.IndexOf("TRA", StringComparison.CurrentCulture) != -1)
				return LevelFlags.Trace;

			return LevelFlags.Other;
		}

		#region Implementation of ILogEntryParser

		/// <summary>
		///     This method is called for every log entry of the source log file
		///     and we want to detect our custom log levels which aren't natively detected by Tailviewer.
		/// </summary>
		/// <remarks>
		///     This method is called in an unspecified order. This means that Tailviewer is not obligated to linearly
		///     scan a log file and feed this class each log entry successively. It may do so at times, but if you
		///     rely on this then your plugin will break with future Tailviewer versions. IF you need to rely on the order
		///     of log entries given to you, then you should implement <see cref="ILogSourceParserPlugin"/> instead.
		/// </remarks>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		public IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry)
		{
			var parsedLogEntry = new LogEntry(logEntry);

			// We do this by inspecting the "RawContent" field of the original log entry
			// which is, for text log files, the raw string of the log line without line endings,
			// and then match our strings to log levels that Tailviewer understands.
			var logLevel = GetLogLevel(logEntry.RawContent);

			// Finally, we assign the log level to the parsed log entry
			parsedLogEntry.LogLevel = logLevel;

			// which we then return back to Tailviewer.
			return parsedLogEntry;
		}

		public IEnumerable<IColumnDescriptor> Columns
		{
			get { return new IColumnDescriptor[] {GeneralColumns.LogLevel}; }
		}

		#endregion
	}
}