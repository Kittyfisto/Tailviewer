using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;

namespace Tailviewer.Ui.ViewModels
{
	public interface IDataSourceViewModel
		: INotifyPropertyChanged
	{
		ICommand OpenInExplorerCommand { get; }

		string DisplayName { get; }

		int TotalCount { get; }

		int OtherCount { get; }

		int DebugCount { get; }

		int InfoCount { get; }

		int WarningCount { get; }

		int NoTimestampCount { get; }

		int ErrorCount { get; }

		int FatalCount { get; }

		Size FileSize { get; }
		bool IsVisible { get; set; }
		LogLineIndex VisibleLogLine { get; set; }
		double HorizontalOffset { get; set; }

		HashSet<LogLineIndex> SelectedLogLines { get; set; }

		TimeSpan LastWrittenAge { get; }

		ICommand RemoveCommand { get; }

		bool FollowTail { get; set; }

		bool ShowLineNumbers { get; set; }

		bool ColorByLevel { get; set; }

		#region Search

		string SearchTerm { get; set; }
		ICommand StartSearchCommand { get; }
		ICommand StopSearchCommand { get; }
		IEnumerable<LogMatch> SearchMatches { get; }
		int SearchMatchCount { get; }
		int CurrentMatchIndex { get; set; }

		#endregion

		DateTime LastViewed { get; }

		IDataSource DataSource { get; }

		LevelFlags LevelsFilter { get; set; }
		IDataSourceViewModel Parent { get; set; }
		IEnumerable<ILogEntryFilter> QuickFilterChain { get; set; }

		event Action<IDataSourceViewModel> Remove;
		void Update();
	}
}