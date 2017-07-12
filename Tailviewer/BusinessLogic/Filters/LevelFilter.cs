using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
{
	/// <summary>
	///     A filter that can be used to exclude entries of certain levels.
	/// </summary>
	public sealed class LevelFilter
		: ILogEntryFilter
	{
		public readonly LevelFlags Level;

		public LevelFilter(LevelFlags level)
		{
			Level = level;
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
// ReSharper disable LoopCanBeConvertedToQuery
			foreach (LogLine logLine in logEntry)
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

			if (logLine.Level != LevelFlags.None)
				return false;

			return true;
		}

		public List<LogLineMatch> Match(LogLine line)
		{
			return new List<LogLineMatch>();
		}

		public void Match(LogLine line, List<LogLineMatch> matches)
		{
			
		}
	}
}