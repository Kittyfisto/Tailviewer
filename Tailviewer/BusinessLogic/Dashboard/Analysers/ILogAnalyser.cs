using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic.Dashboard.Analysers
{
	/// <summary>
	///     The interface for an analyser, responsible for looking at a <see cref="ILogFile" /> or <see cref="ILogTable" />
	///     and producing a result. The result should be updated when the log file or table changes.
	/// </summary>
	public interface ILogAnalyser
		: ILogFileListener
		, ILogTableListener
		, IDisposable
	{
		/// <summary>
		/// The most recent unexpected exceptions thrown by this analyser.
		/// </summary>
		IReadOnlyList<Exception> UnexpectedExceptions { get; }

			/// <summary>
		///     The total amount of time spent on this specific analyser.
		/// </summary>
		TimeSpan AnalysisTime { get; }
	}
}