using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tailviewer.Core.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for maintaining the list of active analyses, analysis snapshots and -templates.
	/// </summary>
	public sealed class AnalysisEngine
	{
		private readonly List<ActiveAnalysis> _activeAnalyses;
		private readonly List<AnalysisSnapshotHandle> _analysisSnapshots;
		private readonly ILogAnalyserEngine _logAnalyserEngine;
		private readonly object _syncRoot;
		private readonly ITaskScheduler _taskScheduler;
		private readonly ISerialTaskScheduler _ioScheduler;

		public AnalysisEngine(ITaskScheduler taskScheduler,
			ISerialTaskScheduler ioScheduler,
			ILogAnalyserEngine logAnalyserEngine)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (ioScheduler == null)
				throw new ArgumentNullException(nameof(ioScheduler));
			if (logAnalyserEngine == null)
				throw new ArgumentNullException(nameof(logAnalyserEngine));

			_taskScheduler = taskScheduler;
			_ioScheduler = ioScheduler;
			_logAnalyserEngine = logAnalyserEngine;
			_syncRoot = new object();

			_activeAnalyses = new List<ActiveAnalysis>();
			_analysisSnapshots = new List<AnalysisSnapshotHandle>();

			_taskScheduler.StartPeriodic(TriggerSnapshotFolderScan, TimeSpan.FromSeconds(5));
		}

		private void TriggerSnapshotFolderScan()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IAnalysis> Active
		{
			get
			{
				lock (_syncRoot)
				{
					return _activeAnalyses.ToList();
				}
			}
		}

		public IEnumerable<AnalysisSnapshotHandle> Snapshots
		{
			get
			{
				lock (_syncRoot)
				{
					return _analysisSnapshots.ToList();
				}
			}
		}

		public IAnalysis CreateNewAnalysis(AnalysisTemplate template)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));

			var analyser = new ActiveAnalysis(template,
				_taskScheduler,
				_logAnalyserEngine,
				TimeSpan.FromMilliseconds(value: 100));

			lock (_syncRoot)
			{
				_activeAnalyses.Add(analyser);
			}

			return analyser;
		}

		public AnalysisSnapshotHandle CreateSnapshot(IAnalysis analysis)
		{
			var tmp = analysis as ActiveAnalysis;
			if (tmp == null)
				throw new ArgumentException("It makes no sense to create a snapshot from anything else but an active analysis",
					nameof(analysis));

			var snapshot = tmp.CreateSnapshot();
			var handle = AnalysisSnapshotHandle.FromSnapshot(snapshot);

			lock (_syncRoot)
			{
				_analysisSnapshots.Add(handle);
			}

			StoreSnapshotAsync();

			return handle;
		}

		private void StoreSnapshotAsync()
		{
			// TODO: Serialize snapshot and store on disk...
			throw new NotImplementedException();
		}

		public Task<IAnalysis> LoadSnapshot(AnalysisSnapshotHandle handle)
		{
			// Actually loads the snapshot from disk
			throw new NotImplementedException();
		}

		public bool Remove(IAnalysis analysis)
		{
			var active = analysis as ActiveAnalysis;
			if (active != null)
				lock (_syncRoot)
				{
					return _activeAnalyses.Remove(active);
				}

			return false;
		}
	}
}