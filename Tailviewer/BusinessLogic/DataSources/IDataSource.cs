using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Core.Filters;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public interface IDataSource
		: IDisposable
	{
		/// <summary>
		///     The list of filters as produced by the "quick filter" panel.
		/// </summary>
		IEnumerable<ILogEntryFilter> QuickFilterChain { get; set; }

		/// <summary>
		///     The actual log file this source eventually represents,
		///     WITHOUT any of the settings of this source applied
		///     (such as multiline, filters, etc...).
		/// </summary>
		ILogFile OriginalLogFile { get; }

		/// <summary>
		///     A log file which has the <see cref="IsSingleLine" /> modification
		///     applied, but is still unfiltered.
		/// </summary>
		ILogFile UnfilteredLogFile { get; }

		/// <summary>
		///     The final log file with all the modifications of this data source
		///     applied to it.
		/// </summary>
		ILogFile FilteredLogFile { get; }

		ILogFileSearch Search { get; }

		DateTime? LastModified { get; }
		DateTime LastViewed { get; set; }

		string FullFileName { get; }
		bool FollowTail { get; set; }
		bool ShowLineNumbers { get; set; }
		bool ShowDeltaTimes { get; set; }
		bool ShowElapsedTime { get; set; }
		string SearchTerm { get; set; }
		LevelFlags LevelFilter { get; set; }
		HashSet<LogLineIndex> SelectedLogLines { get; set; }
		LogLineIndex VisibleLogLine { get; set; }
		double HorizontalOffset { get; set; }
		DataSource Settings { get; }
		int TotalCount { get; }
		Size FileSize { get; }
		bool ColorByLevel { get; set; }
		bool HideEmptyLines { get; set; }
		bool IsSingleLine { get; set; }
		DataSourceId Id { get; }
		DataSourceId ParentId { get; }

		/// <summary>
		///     A one or two digit character code representing this data source.
		/// </summary>
		string CharacterCode { get; set; }

		#region Counts

		int NoLevelCount { get; }
		int TraceCount { get; }
		int DebugCount { get; }
		int InfoCount { get; }
		int WarningCount { get; }
		int ErrorCount { get; }
		int FatalCount { get; }
		int NoTimestampCount { get; }

		#endregion

		#region QuickFilters

		void ActivateQuickFilter(QuickFilterId id);
		bool DeactivateQuickFilter(QuickFilterId id);
		bool IsQuickFilterActive(QuickFilterId id);

		#endregion

		#region Ananlyses

		void EnableAnalysis(AnalysisId id);
		void DisableAnalysis(AnalysisId id);
		bool IsAnalysisActive(AnalysisId id);

		#endregion
	}
}