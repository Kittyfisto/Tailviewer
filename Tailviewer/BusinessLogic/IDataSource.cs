using System;
using System.Collections.Generic;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
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
		int OtherCount { get; }
		int DebugCount { get; }
		int InfoCount { get; }
		int WarningCount { get; }
		int ErrorCount { get; }
		int FatalCount { get; }
		string FullFileName { get; set; }
		bool FollowTail { get; set; }
		string StringFilter { get; set; }
		LevelFlags LevelFilter { get; set; }
		LogLineIndex SelectedLogLine { get; set; }
		LogLineIndex VisibleLogLine { get; set; }
		DataSource Settings { get; }
		int TotalCount { get; }
		Size FileSize { get; }
		bool ColorByLevel { get; set; }
		Guid Id { get; }
		Guid ParentId { get; }
		void ActivateQuickFilter(Guid id);
		bool DeactivateQuickFilter(Guid id);
		bool IsQuickFilterActive(Guid id);
	}
}