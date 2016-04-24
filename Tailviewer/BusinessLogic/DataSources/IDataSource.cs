using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public interface IDataSource : IDisposable
	{
		/// <summary>
		///     The list of filters as produced by the "quick filter" panel.
		/// </summary>
		IEnumerable<ILogEntryFilter> QuickFilterChain { get; set; }

		ILogFile LogFile { get; }
		ILogFile FilteredLogFile { get; }

		DateTime LastModified { get; }
		DateTime LastViewed { get; set; }

		string FullFileName { get; }
		bool FollowTail { get; set; }
		bool ShowLineNumbers { get; set; }
		string StringFilter { get; set; }
		LevelFlags LevelFilter { get; set; }
		HashSet<LogLineIndex> SelectedLogLines { get; set; }
		LogLineIndex VisibleLogLine { get; set; }
		DataSource Settings { get; }
		int TotalCount { get; }
		Size FileSize { get; }
		bool ColorByLevel { get; set; }
		Guid Id { get; }
		Guid ParentId { get; }

		#region Counts

		int NoLevelCount { get; }
		int DebugCount { get; }
		int InfoCount { get; }
		int WarningCount { get; }
		int ErrorCount { get; }
		int FatalCount { get; }
		int NoTimestampCount { get; }

		#endregion

		void ActivateQuickFilter(Guid id);
		bool DeactivateQuickFilter(Guid id);
		bool IsQuickFilterActive(Guid id);
	}
}