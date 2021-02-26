﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Ui.ViewModels.ContextMenu;

namespace Tailviewer.Ui.ViewModels
{
	public interface IDataSourceViewModel
		: INotifyPropertyChanged
	{
		IPluginDescription TranslationPlugin { get; }

		/// <summary>
		///     The list of menu items which are displayed in the "File" menu of the main window.
		/// </summary>
		IEnumerable<IMenuViewModel> FileMenuItems { get; }

		/// <summary>
		///     The list of menu items which are displayed in the "View" menu of the main window.
		/// </summary>
		IEnumerable<IMenuViewModel> ViewMenuItems { get; }

		/// <summary>
		///     The name of this data source as presented to the user.
		/// </summary>
		/// <remarks>
		///     The setter will throw an <see cref="InvalidOperationException" /> if <see cref="CanBeRenamed" />
		///     is set to false.
		/// </remarks>
		string DisplayName { get; set; }

		/// <summary>
		///     True when <see cref="DisplayName" /> may be changed, false otherwise.
		/// </summary>
		bool CanBeRenamed { get; }

		/// <summary>
		///     A user-readable description of the origin of the data source.
		///     Could be a simpel filesystem path, a URL, etc...
		/// </summary>
		string DataSourceOrigin { get; }

		bool Exists { get; }

		int TotalCount { get; }

		int OtherCount { get; }

		int TraceCount { get; }

		int DebugCount { get; }

		int InfoCount { get; }

		int WarningCount { get; }

		int NoTimestampCount { get; }

		int ErrorCount { get; }

		int FatalCount { get; }

		Size? FileSize { get; }
		bool IsVisible { get; set; }
		LogLineIndex VisibleLogLine { get; set; }
		double HorizontalOffset { get; set; }

		HashSet<LogLineIndex> SelectedLogLines { get; set; }

		TimeSpan? LastWrittenAge { get; }

		ICommand RemoveCommand { get; }

		bool FollowTail { get; set; }

		bool ShowLineNumbers { get; set; }

		bool ShowDeltaTimes { get; set; }

		bool ShowElapsedTime { get; set; }

		bool ColorByLevel { get; set; }

		bool HideEmptyLines { get; set; }

		bool IsSingleLine { get; set; }

		/// <summary>
		///     When set to true, all current log entries will be filtered out.
		///     Newer log entries will still appear.
		///     When set to false, all log entries (that are not otherwise filtered) appear again.
		/// </summary>
		bool ScreenCleared { get; }

		/// <summary>
		/// Clears all log entries currently part of the data source.
		/// Future log entries will be shown once they become available.
		/// </summary>
		ICommand ClearScreenCommand { get; }

		/// <summary>
		/// Sets <see cref="ScreenCleared"/> to false.
		/// </summary>
		ICommand ShowAllCommand { get; }

		double Progress { get; }

		DateTime LastViewed { get; }

		IDataSource DataSource { get; }

		LevelFlags LevelsFilter { get; set; }
		IDataSourceViewModel Parent { get; set; }
		IEnumerable<ILogEntryFilter> QuickFilterChain { get; set; }

		/// <summary>
		/// 
		/// </summary>
		IEnumerable<IMenuViewModel> ContextMenuItems { get; }

		void RequestBringIntoView(LogLineIndex index);

		event Action<IDataSourceViewModel, LogLineIndex> OnRequestBringIntoView;
		event Action<IDataSourceViewModel> Remove;

		void Update();

		#region Search

		ISearchViewModel Search { get; }

		IFindAllViewModel FindAll { get; }

		#endregion
	}
}