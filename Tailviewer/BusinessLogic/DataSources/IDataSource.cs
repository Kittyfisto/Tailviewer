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

		ILogFile UnfilteredLogFile { get; }
		ILogFile FilteredLogFile { get; }
		ILogFileSearch Search { get; }

		DateTime LastModified { get; }
		DateTime LastViewed { get; set; }

		string FullFileName { get; }
		bool FollowTail { get; set; }
		bool ShowLineNumbers { get; set; }
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