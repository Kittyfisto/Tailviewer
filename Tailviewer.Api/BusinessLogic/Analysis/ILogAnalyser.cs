using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     The interface for an analyser, responsible for looking at a <see cref="ILogFile" />
	///     and producing a result. The result should be updated when the log file or table changes.
	/// </summary>
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
		///     The current result of this analysis.
		/// </summary>
		/// <remarks>
		///     Is queried every now and then from a background thread.
		///     So implementations will have to make sure that accessing this property
		///     does not introduce race conditions.
		/// </remarks>
		ILogAnalysisResult Result { get; }

		/// <summary>
		///     The current progress of this analysis.
		/// </summary>
		/// <remarks>
		///     Is queried every now and then from a background thread.
		///     So implementations will have to make sure that accessing this property
		///     does not introduce race conditions.
		/// </remarks>
		Percentage Progress { get; }
	}
}