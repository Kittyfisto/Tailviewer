using System.Collections.Generic;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	/// A filter that can be used to exclude entries of certain levels.
	/// </summary>
	internal sealed class LevelFilter
		: ILogEntryFilter
	{
		public readonly LevelFlags Level;
		public readonly bool ExcludeOther;

		public LevelFilter(LevelFlags level, bool excludeOther)
		{
			Level = level;
			ExcludeOther = excludeOther;
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
// ReSharper disable LoopCanBeConvertedToQuery
			foreach (var logLine in logEntry)
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (PassesFilter(logLine))
					return true;
			}

			return false;
		}

		public bool PassesFilter(LogLine logLine)
		{
			if ((logLine.Level & Level) != 0)
				return true;

			if (logLine.Level != LevelFlags.None || ExcludeOther)
				return false;

			return true;
		}
	}
}