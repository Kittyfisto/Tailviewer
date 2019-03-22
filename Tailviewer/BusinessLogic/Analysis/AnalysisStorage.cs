using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogAnalyserEngine _logAnalyserEngine;
		private readonly SnapshotsWatchdog _snapshots;
		private readonly object _syncRoot;
		private readonly ITaskScheduler _taskScheduler;
		private readonly List<ActiveAnalysisConfiguration> _templates;
		private readonly Dictionary<AnalysisId, ActiveAnalysis> _analyses;
		private readonly Dictionary<AnalysisId, ActiveAnalysisConfiguration> _lastSavedAnalyses;
		private readonly IFilesystem _filesystem;
		private readonly ITypeFactory _typeFactory;

		public AnalysisStorage(ITaskScheduler taskScheduler,
			IFilesystem filesystem,
			ILogAnalyserEngine logAnalyserEngine,
			ITypeFactory typeFactory)
		{
			_taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
			_logAnalyserEngine = logAnalyserEngine ?? throw new ArgumentNullException(nameof(logAnalyserEngine));
			_filesystem = filesystem ?? throw new ArgumentNullException(nameof(filesystem));
			_typeFactory = typeFactory ?? throw new ArgumentNullException(nameof(typeFactory));
			_syncRoot = new object();

			_snapshots = new SnapshotsWatchdog(taskScheduler, filesystem, typeFactory);
			_templates = new List<ActiveAnalysisConfiguration>();
			_analyses = new Dictionary<AnalysisId, ActiveAnalysis>();
			_lastSavedAnalyses = new Dictionary<AnalysisId, ActiveAnalysisConfiguration>();

			// TODO: Maybe don't block in the ctor in the future?
			// If we do that, the public interface will have to be changed
			RestoreSavedAnalysesAsync().Wait();
		}

		private async Task RestoreSavedAnalysesAsync()
		{
			try
			{
				var files = await EnumerateAnalysesAsync();
				foreach (var filePath in files)
				{
					using (var stream = await _filesystem.OpenRead(filePath))
					{
						var configuration = ReadAnalysis(stream);
						if (configuration != null)
						{
							var analysis = new ActiveAnalysis(configuration.Id,
								configuration.Template,
								_taskScheduler,
								_logAnalyserEngine,
								TimeSpan.FromMilliseconds(100));

							try
							{
								lock (_syncRoot)
								{
									_templates.Add(configuration);
									_lastSavedAnalyses.Add(analysis.Id, configuration);

									_analyses.Add(analysis.Id, analysis);
								}
							}
							catch (Exception)
							{
								analysis
									.Dispose(); //< ActiveAnalysis actually spawns new analyses on the engine so we should cancel those in case an exception si thrown here...
								throw;
							}
						}
					}
				}
			}
			catch (DirectoryNotFoundException e)
			{
				Log.DebugFormat("Unable to restore existing analyses: {0}", e);
				await CreateAnalysisFolderAsync();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to restore existing analyses: {0}", e);
			}
		}

		private async Task CreateAnalysisFolderAsync()
		{
			try
			{
				await _filesystem.CreateDirectory(Constants.AnalysisDirectory);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to create analysis folder: {0}", e);
			}
		}

		[Pure]
		private async Task<IReadOnlyList<string>> EnumerateAnalysesAsync()
		{
			var filter = string.Format("*.{0}", Constants.AnalysisExtension);
			return await _filesystem.EnumerateFiles(Constants.AnalysisDirectory, filter);
		}

		public void Dispose()
		{
			_snapshots?.Dispose();
		}

		public IEnumerable<ActiveAnalysisConfiguration> AnalysisTemplates
		{
			get
			{
				lock (_syncRoot)
				{
					return _templates.ToList();
				}
			}
		}

		public IEnumerable<IAnalysis> Analyses
		{
			get
			{
				lock (_syncRoot)
				{
					return _analyses.Values.ToList();
				}
			}
		}

		/// <inheritdoc />
		public Task SaveAsync(AnalysisId id)
		{
			ActiveAnalysisConfiguration config;
			if (!TryGetTemplateFor(id, out config))
			{
				Log.WarnFormat("Unable to find configuration with id '{0}', it will not be saved!", id);
				return Task.FromResult(42);
			}

			return SaveAsync(config);
		}

		public bool TryGetAnalysisFor(AnalysisId id, out IAnalysis analysis)
		{
			lock (_syncRoot)
			{
				ActiveAnalysis tmp;
				if (_analyses.TryGetValue(id, out tmp))
				{
					analysis = tmp;
					return true;
				}
			}

			analysis = null;
			return false;
		}

		public bool TryGetTemplateFor(AnalysisId analysisId, out ActiveAnalysisConfiguration configuration)
		{
			lock (_syncRoot)
			{
				return _lastSavedAnalyses.TryGetValue(analysisId, out configuration);
			}
		}

		public IAnalysis CreateAnalysis(AnalysisTemplate template, AnalysisViewTemplate viewTemplate)
		{
			ActiveAnalysisConfiguration analysisConfiguration;

			var id = AnalysisId.CreateNew();
			var analysis = new ActiveAnalysis(id,
			                                  template,
			                                  _taskScheduler,
			                                  _logAnalyserEngine,
			                                  TimeSpan.FromMilliseconds(100));

			try
			{
				analysisConfiguration = new ActiveAnalysisConfiguration(id, template, viewTemplate);
				lock (_syncRoot)
				{
					_templates.Add(analysisConfiguration);
					_analyses.Add(analysis.Id, analysis);
					_lastSavedAnalyses.Add(analysis.Id, analysisConfiguration);
				}
			}
			catch (Exception)
			{
				analysis.Dispose(); //< ActiveAnalysis actually spawns new analyses on the engine so we should cancel those in case an exception si thrown here...
				throw;
			}

			SaveAsync(analysisConfiguration).Wait();

			return analysis;
		}

		public Task SaveAsync(ActiveAnalysisConfiguration analysisConfiguration)
		{
			var filename = GetFilename(analysisConfiguration.Id);
			var directory = Path.GetDirectoryName(filename);

			// We don't know if the directory exists, so we'll just create
			// it beforehand...
			_filesystem.CreateDirectory(directory);
			// TODO: Clone the analysis
			return _filesystem.OpenWrite(filename).ContinueWith(x => WriteAnalysis(x, analysisConfiguration), TaskContinuationOptions.AttachedToParent);
		}

		public void Remove(AnalysisId id)
		{
			lock (_syncRoot)
			{
				_templates.RemoveAll(x => x.Id == id);
				if (_analyses.TryGetValue(id, out var analysis))
				{
					analysis.Dispose();
					_analyses.Remove(id);

					var filename = GetFilename(id);
					_filesystem.DeleteFile(filename);
				}
				else
				{
					Log.WarnFormat("Unable to remove analysis with id '{0}', it doesn't match any of the existing {1} analyses",
					               id,
					               _analyses.Count);
				}
			}
		}

		internal static string GetFilename(AnalysisId id)
		{
			var fname = string.Format("{0}.{1}", id, Constants.AnalysisExtension);
			var filename = Path.Combine(Constants.AnalysisDirectory, fname);
			return filename;
		}

		private void WriteAnalysis(Task<Stream> task, ActiveAnalysisConfiguration analysisConfiguration)
		{
			using (var stream = task.Result)
			{
				// Just in case we're writing over an existing file...
				stream.SetLength(0);
				using (var writer = new Writer(stream, _typeFactory))
				{
					writer.WriteAttribute("Analysis", analysisConfiguration);
				}
			}
		}

		public ActiveAnalysisConfiguration ReadAnalysis(Stream stream)
		{
			var reader = new Reader(stream, _typeFactory);
			reader.TryReadAttribute("Analysis", out ActiveAnalysisConfiguration analysis);
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
					using (var writer = new Writer(stream, _typeFactory))
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