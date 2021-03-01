using System;

namespace Tailviewer
{
	/// <summary>
	///     The interface for a parser that is responsible for determining the timestamp of a message
	///     of a log line.
	/// </summary>
	public interface ITimestampParser
	{
		/// <summary>
		///     The minimum length (in characters) of the timestamp this parser is able to parse.
		/// </summary>
		int MinimumLength { get; }

		/// <summary>
		/// </summary>
		/// <param name="content"></param>
		/// <param name="timestamp"></param>
		/// <returns></returns>
		bool TryParse(string content, out DateTime timestamp);
	}
}