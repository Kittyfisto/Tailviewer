using System;

namespace Tailviewer.LogLevelPlugin
{
	public sealed class MyCustomLogLevelTranslator
		: ILogLineTranslator
	{
		#region Implementation of ILogLineTranslator

		public LogLine Translate(ILogSource logSource, LogLine line)
		{
			line.Level = GetLogLevel(line.Message);
			return line;
		}

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

		#endregion
	}
}