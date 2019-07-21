using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     Responsible for parsing a particular text log file so that tailviewer can interpret
	///     the contents properly.
	/// </summary>
	public interface ITextLogFileParser
		: IDisposable
	{
		/// <summary>
		///     The minimum set of columns tailviewer can expected in the log file.
		/// </summary>
		IReadOnlyList<ILogFileColumn> Minimum { get; }

		/// <summary>
		///     Parses the given log entry and returns new log entry with as many columns as could be parsed.
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry);
	}
}