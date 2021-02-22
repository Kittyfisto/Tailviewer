using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     Responsible for parsing a particular log file so that tailviewer can interpret
	///     the contents properly.
	/// </summary>
	public interface ILogEntryParser
		: IDisposable
	{
		/// <summary>
		///     Parses the given log entry and returns new log entry with as many columns as could be parsed.
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		[Pure]
		[ThreadSafe]
		IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry);

		/// <summary>
		///     The set of columns this parser can detect.
		/// </summary>
		[ThreadSafe]
		IEnumerable<IColumnDescriptor> Columns { get; }
	}
}