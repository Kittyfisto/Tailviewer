using System;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     Responsible for parsing a particular text log file so that tailviewer can interpret
	///     the contents properly.
	/// </summary>
	public interface ITextLogFileParser
		: IDisposable
	{
		/// <summary>
		///     Parses the given log entry and returns new log entry with as many columns as could be parsed.
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry);
	}
}