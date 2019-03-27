using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     The interface for an object, responsible for translating an incoming <see cref="LogLine" />
	///     into another (or an equal) <see cref="LogLine" />.
	/// </summary>
	/// <remarks>
	///     This interface may be implemented in plugins
	/// </remarks>
	public interface ILogLineTranslator
	{
		/// <summary>
		///     Translates the given line into whatever form the subclass deems useful.
		/// </summary>
		/// <param name="logFile">The log file which requests the given line to be translated</param>
		/// <param name="line"></param>
		/// <returns>The new line that shall be exposed by this log file</returns>
		[Pure]
		LogLine Translate(ILogFile logFile, LogLine line);
	}
}