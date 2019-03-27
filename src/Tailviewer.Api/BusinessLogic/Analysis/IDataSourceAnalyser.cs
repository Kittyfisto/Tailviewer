using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a live analysis or a snapshot (depending on <see cref="IsFrozen" />) of a
	///     previous analysis.
	///     When this is a live analysis, then changes to <see cref="Configuration" /> are eventually
	///     forwarded to a <see cref="ILogAnalyser" />.
	/// </summary>
	/// <remarks>
	///     TODO: Remove properties from this interface which plugins would not want to implement
	/// </remarks>
	public interface IDataSourceAnalyser
		: IDisposable
	{
		/// <summary>
		///     A unique id which identifies this analyser.
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property from this interface, it should probably only be part of a more specialized interface
		/// </remarks>
		AnalyserId Id { get; }

		/// <summary>
		///     The id of the factory which created this analyser.
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property from this interface, it should probably only be part of a more specialized interface
		/// </remarks>
		AnalyserPluginId AnalyserPluginId { get; }

		/// <summary>
		///     The current progress of the analysis.
		/// </summary>
		Percentage Progress { get; }

		/// <summary>
		///     The result of the analysis.
		///     May change over the lifetime of this analyser.
		///     May be null.
		/// </summary>
		ILogAnalysisResult Result { get; }

		/// <summary>
		///     Whether or not this analyser is frozen.
		///     A frozen analyser may not be modified and thus changing
		///     the configuration is not allowed.
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property from this interface, it should probably only be part of a more specialized interface
		/// </remarks>
		bool IsFrozen { get; }

		/// <summary>
		///     The current configuration used by the analyser.
		///     When the configuration is changed (by calling the setter),
		///     then the analysis is restarted using the new configuration
		///     and <see cref="Result" /> will eventually represent the result
		///     of the analysis using the new configuration.
		/// </summary>
		ILogAnalyserConfiguration Configuration { get; set; }

		/// <summary>
		///     This method is called whenever a new log file is to be analysed.
		/// </summary>
		/// <remarks>
		///     This method doesn't need to do anything unless the log file supplied through
		///     <see cref="IDataSourceAnalyserPlugin.Create" />
		///     is NOT being used in which case an analyser will want to start analysing the given log file.
		/// </remarks>
		/// <param name="logFile"></param>
		void OnLogFileAdded(ILogFile logFile);
		
		/// <summary>
		///     This method is called whenever log file should no longer be analysed.
		/// </summary>
		/// <remarks>
		///     This method doesn't need to do anything unless the log file supplied through
		///     <see cref="IDataSourceAnalyserPlugin.Create" />
		///     is NOT being used in which case an analyser will want to stop analysing thatthe given log file.
		/// </remarks>
		/// <param name="logFile"></param>
		void OnLogFileRemoved(ILogFile logFile);
	}
}