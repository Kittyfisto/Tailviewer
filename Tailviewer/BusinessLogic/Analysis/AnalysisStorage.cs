using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tailviewer.Core;
using Tailviewer.Core.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for maintaining the list of active analyses, analysis snapshots and -templates.
	/// </summary>
	public sealed class AnalysisStorage
		: IAnalysisStorage
		, IDisposable
	{
		private readonly List<ActiveAnalysis> _activeAnalyses;
		private readonly ILogAnalyserEngine _logAnalyserEngine;
		private readonly SnapshotsWatchdog _snapshots;
		private readonly object _syncRoot;
		private readonly ITaskScheduler _taskScheduler;

		public AnalysisStorage(ITaskScheduler taskScheduler,
			IFilesystem filesystem,
			ILogAnalyserEngine logAnalyserEngine,
			ITypeFactory typeFactory = null)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (filesystem == null)
				throw new ArgumentNullException(nameof(filesystem));
			if (logAnalyserEngine == null)
				throw new ArgumentNullException(nameof(logAnalyserEngine));

			_taskScheduler = taskScheduler;
			_logAnalyserEngine = logAnalyserEngine;
			_syncRoot = new object();

			_activeAnalyses = new List<ActiveAnalysis>();
			_snapshots = new SnapshotsWatchdog(taskScheduler, filesystem, typeFactory);
		}

		public void Dispose()
		{
			_snapshots?.Dispose();
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

		public IEnumerable<IAnalysis> Analyses
		{
			get
			{
				lock (_syncRoot)
				{
					return _activeAnalyses.ToList();
				}
			}
		}

		public IAnalysis CreateAnalysis(AnalysisTemplate template)
		{
			var analysis = new ActiveAnalysis(template,
				_taskScheduler,
				_logAnalyserEngine,
				TimeSpan.FromMilliseconds(100));
			return analysis;
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateSnapshots()
		{
			return _snapshots.EnumerateSnapshots();
		}

		/// <inheritdoc />
		public Task SaveSnapshot(IAnalysis analysis, AnalysisViewTemplate viewTemplate)
		{
			var tmp = analysis as ActiveAnalysis;
			if (tmp == null)
				throw new ArgumentException("It makes no sense to create a snapshot from anything else but an active analysis",
					nameof(analysis));

			var analysisSnapshot = tmp.CreateSnapshot();
			var template = new AnalysisTemplate(analysisSnapshot.Analysers.Select(x => new AnalyserTemplate
			{
				Id = x.Id,
				FactoryId = x.FactoryId,
				Configuration = x.Configuration
			}));
			var clone = viewTemplate.Clone();
			var results = analysisSnapshot.Analysers.Select(x => new AnalyserResult
			{
				AnalyserId = x.Id,
				Result = x.Result
			}).ToList();
			var snapshot = new Core.Analysis.AnalysisSnapshot(template, clone, results);
			return _snapshots.Save(snapshot);
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

		private sealed class SnapshotsWatchdog
			: IDisposable
		{
			private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			private readonly ITaskScheduler _taskScheduler;
			private readonly IFilesystem _filesystem;
			private readonly object _syncRoot;
			private readonly ITypeFactory _typeFactory;

			public SnapshotsWatchdog(ITaskScheduler taskScheduler, IFilesystem filesystem, ITypeFactory typeFactory = null)
			{
				if (taskScheduler == null)
					throw new ArgumentNullException(nameof(taskScheduler));
				if (filesystem == null)
					throw new ArgumentNullException(nameof(filesystem));

				_syncRoot = new object();
				_taskScheduler = taskScheduler;
				_filesystem = filesystem;
				_typeFactory = typeFactory;
			}

			public void Dispose()
			{
			}

			public Task<IReadOnlyList<string>> EnumerateSnapshots()
			{
				var pattern = string.Format("*.{0}", Constants.SnapshotExtension);
				return _filesystem.EnumerateFiles(Constants.SnapshotDirectory, pattern);
			}

			public Task Save(Core.Analysis.AnalysisSnapshot snapshot)
			{
				var fileName = DetermineFilename(snapshot);
				return SaveAsync(fileName, snapshot);
			}

			private Task SaveAsync(string filePath, Core.Analysis.AnalysisSnapshot snapshot)
			{
				var directory = Path.GetDirectoryName(filePath);

				// 
				_filesystem.CreateDirectory(directory);
				return _filesystem.OpenWrite(filePath).ContinueWith(x => WriteAnalysisSnapshot(x, snapshot));
			}

			private void WriteAnalysisSnapshot(Task<Stream> task, Core.Analysis.AnalysisSnapshot snapshot)
			{
				try
				{
					using (var stream = task.Result)
					using (var writer = new Writer(stream))
					{
						writer.WriteAttribute("Snapshot", snapshot);
					}
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while trying to write snapshot to disk: {0}", e);
				}
			}

			private string DetermineFilename(Core.Analysis.AnalysisSnapshot snapshot)
			{
				var filename = string.Format("Snapshot.{0}", Constants.SnapshotExtension);
				var filepath = Path.Combine(Constants.SnapshotDirectory, filename);
				return filepath;
			}
		}
	}
}