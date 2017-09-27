using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogTables;

namespace Tailviewer.BusinessLogic.Analysis.Analysers
{
	/// <summary>
	///     The interface for an analyser, responsible for looking at a <see cref="ILogFile" /> or <see cref="ILogTable" />
	///     and producing a result. The result should be updated when the log file or table changes.
	/// </summary>
	/// <remarks>
	///     The constructor of a <see cref="ILogAnalyser" /> is expected to take two parameters of types
	///     <see cref="ILogFile" /> and
	///     <see cref="ILogAnalyserConfiguration" /> in that order.
	/// </remarks>
	public interface ILogAnalyser
		: ILogFileListener
		, IDisposable
	{
		/// <summary>
		///     The most recent unexpected exceptions thrown by this analyser.
		/// </summary>
		IReadOnlyList<Exception> UnexpectedExceptions { get; }

		/// <summary>
		///     The total amount of time spent on this specific analyser.
		/// </summary>
		TimeSpan AnalysisTime { get; }

		/// <summary>
		/// 
		/// </summary>
		ILogAnalysisResult Result { get; }

		/// <summary>
		/// 
		/// </summary>
		Percentage Progress { get; }
	}
}