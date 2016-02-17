using System;
using System.ComponentModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	internal interface IDataSourceViewModel
		: INotifyPropertyChanged
	{
		ICommand OpenInExplorerCommand { get; }

		string DisplayName { get; }

		int TotalCount { get; }

		int OtherCount { get; }

		int DebugCount { get; }

		int InfoCount { get; }

		int WarningCount { get; }

		int ErrorCount { get; }

		int FatalCount { get; }

		Size FileSize { get; }

		LogLineIndex VisibleLogLine { get; set; }

		LogLineIndex SelectedLogLine { get; set; }

		TimeSpan LastWrittenAge { get; }

		ICommand RemoveCommand { get; }

		bool FollowTail { get; set; }

		bool ColorByLevel { get; set; }

		string StringFilter { get; set; }

		DateTime LastViewed { get; }

		IDataSource DataSource { get; }

		LevelFlags LevelsFilter { get; set; }
		IDataSourceViewModel Parent { get; set; }

		event Action<IDataSourceViewModel> Remove;
		void Update();
	}
}