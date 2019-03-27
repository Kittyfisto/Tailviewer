using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     The interface for an analyser, responsible for looking at a virtual <see cref="ILogFile" />
	///     and producing a result. The result is updated whenever the log file changes.
	/// </summary>
	/// <remarks>
	///     A <see cref="ILogAnalyser"/> only works with a static <see cref="ILogAnalyserConfiguration"/> and is destroyed and re-created
	///     whenever the configuration changes. If this is unacceptable to you, then you might want to implement
	///     <see cref="IDataSourceAnalyser"/>.
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