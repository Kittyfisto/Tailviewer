using System;

namespace Tailviewer.BusinessLogic
{
	internal interface IDataSource : IDisposable
	{
		DateTime LastModified { get; }
		DateTime LastViewed { get; set; }
		int OtherCount { get; }
		int DebugCount { get; }
		int InfoCount { get; }
		int WarningCount { get; }
		int ErrorCount { get; }
		int FatalCount { get; }
		ILogFile LogFile { get; }
		string FullFileName { get; set; }
		bool IsOpen { get; set; }
		bool FollowTail { get; set; }
		string StringFilter { get; set; }
		LevelFlags LevelFilter { get; set; }
		LogLineIndex SelectedLogLine { get; set; }
		LogLineIndex VisibleLogLine { get; set; }
		Settings.DataSource Settings { get; }
		int TotalCount { get; }
		Size FileSize { get; }
		bool ColorByLevel { get; set; }
		void ActivateQuickFilter(Guid id);
		bool DeactivateQuickFilter(Guid id);
		bool IsQuickFilterActive(Guid id);
	}
}