using System.Text.RegularExpressions;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Formats.Serilog
{
	/// <summary>
	///     Responsible for matching a particular serilog type such as Timestamp, Level or Message.
	/// </summary>
	public interface ISerilogMatcher
	{
		/// <summary>
		///     The regular expression that matches this particular type.
		/// </summary>
		string Regex { get; }

		/// <summary>
		///     The number of regexp groups contained in <see cref="Regex" />.
		/// </summary>
		/// <remarks>
		///     Is required because in the end, one big regex will be used to parse the entire line
		///     at once.
		/// </remarks>
		int NumGroups { get; }

		/// <summary>
		///     The Tailviewer column into which values of this matcher are to be transformed into.
		/// </summary>
		IColumnDescriptor Column { get; }

		/// <summary>
		///     TODO: Change signature to TryMatchInto to allow for failure without throwing exceptions.
		/// </summary>
		/// <param name="match"></param>
		/// <param name="logEntry"></param>
		void MatchInto(Match match, LogEntry logEntry);
	}
}