using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Media;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses
{
	/// <summary>
	///     Represents the side panel of available and actually active analyses, snapshots and templates.
	/// </summary>
	public sealed class AnalysesSidePanel
		: AbstractSidePanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly string PanelId = "Analysis.Analyses";

		private readonly ObservableCollection<AnalysisViewModel> _active;
		private readonly ObservableCollection<AnalysisTemplateViewModel> _available;
		private readonly IDispatcher _dispatcher;
		private readonly ITaskScheduler _taskScheduler;
		private readonly PathComparer _pathComparer;
		private bool _hasActiveAnalyses;
		private bool _hasAvailableAnalyses;

		#region Snapshots

		private IPeriodicTask _snapshotScanTask;
		private readonly Dictionary<string, AnalysisSnapshotItemViewModel> _availableSnapshotsByFileName;
		private readonly ObservableCollection<AnalysisSnapshotItemViewModel> _availableSnapshots;
		private readonly IAnalysisStorage _analysisStorage;
		private bool _loggedIoException;

		#endregion

		public AnalysesSidePanel(IDispatcher dispatcher, ITaskScheduler taskScheduler, IAnalysisStorage analysisStorage)
		{
			if (dispatcher == null)
				throw new ArgumentNullException(nameof(dispatcher));
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (analysisStorage == null)
				throw new ArgumentNullException(nameof(analysisStorage));

			_dispatcher = dispatcher;
			_taskScheduler = taskScheduler;
			_analysisStorage = analysisStorage;
			_pathComparer = new PathComparer();

			_active = new ObservableCollection<AnalysisViewModel>();
			_available = new ObservableCollection<AnalysisTemplateViewModel>();

			_availableSnapshotsByFileName = new Dictionary<string, AnalysisSnapshotItemViewModel>();
			_availableSnapshots = new ObservableCollection<AnalysisSnapshotItemViewModel>();

			UpdateTooltip();
			PropertyChanged += OnPropertyChanged;
		}

		public IEnumerable<AnalysisViewModel> Active => _active;
		public IEnumerable<AnalysisTemplateViewModel> Available => _available;
		public IEnumerable<AnalysisSnapshotItemViewModel> Snapshots => _availableSnapshots;

		public override Geometry Icon => Icons.ChartGantt;

		public override string Id => PanelId;

		public bool HasActiveAnalyses
		{
			get { return _hasActiveAnalyses; }
			private set
			{
				if (value == _hasActiveAnalyses)
					return;

				_hasActiveAnalyses = value;
				EmitPropertyChanged();
			}
		}

		public bool HasAvailableAnalyses
		{
			get { return _hasAvailableAnalyses; }
			private set
			{
				if (value == _hasAvailableAnalyses)
					return;

				_hasAvailableAnalyses = value;
				EmitPropertyChanged();
			}
		}

		private void ScanSnapshotsFolder()
		{
			var searchPattern = string.Format("*.{0}", Constants.SnapshotExtension);
			try
			{
				// Blocking the timer task is on purpose so we don't perform more scans than we actually can
				var files = _analysisStorage.EnumerateSnapshots().AwaitResult().ToList();
				_dispatcher.BeginInvoke(() => Synchronise(files));
			}
			catch (IOException e)
			{
				if (!_loggedIoException)
				{
					Log.WarnFormat("Unable to enumerate snapshots: {0}", e.Message);
					_loggedIoException = true;
				}
				else
				{
					Log.DebugFormat("Unable to enumerate snapshots: {0}", e.Message);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		/// <summary>
		///     Called on the UI thread.
		/// </summary>
		/// <param name="files"></param>
		private void Synchronise(IReadOnlyList<string> files)
		{
			foreach (var fileInfo in files)
			{
				if (!_availableSnapshotsByFileName.ContainsKey(fileInfo))
				{
					var viewModel = new AnalysisSnapshotItemViewModel(fileInfo);
					_availableSnapshotsByFileName.Add(fileInfo, viewModel);
					_availableSnapshots.Add(viewModel);
				}
			}

			if (_availableSnapshots.Count != files.Count)
			{
				foreach (var pair in _availableSnapshotsByFileName.ToList())
				{
					var fname = pair.Key;
					var viewModel = pair.Value;
					if (!files.Any(x => _pathComparer.Equals(x, fname)))
					{
						_availableSnapshotsByFileName.Remove(fname);
						_availableSnapshots.Remove(viewModel);
					}
				}
			}
		}

		private void UpdateTooltip()
		{
			Tooltip = IsSelected
				? "Hide the list of analyses"
				: "Show the list of analyses";
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(IsSelected):
					UpdateTooltip();
					UpdateFolderScan();
					break;
			}
		}

		private void UpdateFolderScan()
		{
			// Folder scans shall only be performed when the panel is selected (and thus visible to the user).
			// If the user doesn't see the panel then we don't really need to know what happened inside
			// the folder...
			if (IsSelected)
			{
				_snapshotScanTask = _taskScheduler.StartPeriodic(ScanSnapshotsFolder,
					TimeSpan.FromSeconds(value: 5),
					"Snapshot Scan");
			}
			else
			{
				_taskScheduler.StopPeriodic(_snapshotScanTask);
				_snapshotScanTask = null;
			}
		}

		public override void Update()
		{
			HasActiveAnalyses = _active.Count > 0;
			HasAvailableAnalyses = _available.Count > 0;
			UpdateQuickInfo();
		}

		public AnalysisViewModel Add(IAnalysis analysis)
		{
			var viewTemplate = new AnalysisViewTemplate();
			var analysisViewModel = new AnalysisViewModel(_dispatcher, viewTemplate, analysis, _analysisStorage);

			_active.Add(analysisViewModel);
			analysisViewModel.OnRemove += AnalysisOnOnRemove;
			Update();

			return analysisViewModel;
		}

		private void AnalysisOnOnRemove(AnalysisViewModel analysis)
		{
			_active.Remove(analysis);
		}

		private void UpdateQuickInfo()
		{
			switch (_active.Count)
			{
				case 0:
					QuickInfo = null;
					break;

				case 1:
					QuickInfo = _active[index: 0].Name;
					break;

				default:
					QuickInfo = string.Format("{0} analyses", _active.Count);
					break;
			}
		}
	}
}