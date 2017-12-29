using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public abstract class AbstractDataSource
		: IDataSource
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;
		private readonly LogFileCounter _counter;
		private readonly TimeSpan _maximumWaitTime;
		private readonly DataSource _settings;
		private readonly LogFileProxy _logFile;
		private readonly LogFileSearchProxy _search;

		private LogFileSearch _currentSearch;
		private ILogFile _filteredLogFile;
		private IEnumerable<ILogEntryFilter> _quickFilterChain;
		private bool _isDisposed;
		private ILogFile _previousUnfilteredLogFile;

		protected AbstractDataSource(ITaskScheduler taskScheduler, DataSource settings, TimeSpan maximumWaitTime)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (settings.Id == DataSourceId.Empty) throw new ArgumentException("settings.Id shall be set to an actually generated id");

			_taskScheduler = taskScheduler;
			_settings = settings;
			_maximumWaitTime = maximumWaitTime;
			_counter = new LogFileCounter();

			_logFile = new LogFileProxy(taskScheduler, maximumWaitTime);
			_search = new LogFileSearchProxy(taskScheduler, _logFile, maximumWaitTime);
			CreateSearch();
		}

		protected ITaskScheduler TaskScheduler => _taskScheduler;

		protected TimeSpan MaximumWaitTime => _maximumWaitTime;

		public ILogFile FilteredLogFile => _logFile;

		public ILogFileSearch Search => _search;

		/// <summary>
		///     The list of filters as produced by the "quick filter" panel.
		/// </summary>
		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _quickFilterChain; }
			set
			{
				if (ReferenceEquals(value, _quickFilterChain))
					return;

				_quickFilterChain = value;
				CreateFilteredLogFile();
			}
		}

		public string SearchTerm
		{
			get { return _settings.SearchTerm; }
			set
			{
				if (value == SearchTerm)
					return;

				_settings.SearchTerm = value;
				CreateSearch();
			}
		}

		public LevelFlags LevelFilter
		{
			get { return _settings.LevelFilter; }
			set
			{
				if (value == LevelFilter)
					return;

				_settings.LevelFilter = value;
				CreateFilteredLogFile();
			}
		}

		public DateTime LastModified => UnfilteredLogFile.LastModified;

		public DateTime LastViewed
		{
			get { return _settings.LastViewed; }
			set { _settings.LastViewed = value; }
		}

		public DataSourceId Id => _settings.Id;

		public DataSourceId ParentId => _settings.ParentId;

		public void ActivateQuickFilter(QuickFilterId id)
		{
			// Should I add a sanity check here?
			_settings.ActivatedQuickFilters.Add(id);
		}

		public bool DeactivateQuickFilter(QuickFilterId id)
		{
			return _settings.ActivatedQuickFilters.Remove(id);
		}

		public bool IsQuickFilterActive(QuickFilterId id)
		{
			return _settings.ActivatedQuickFilters.Contains(id);
		}

		public void EnableAnalysis(AnalysisId id)
		{
			_settings.Analyses.Add(id);
		}

		public void DisableAnalysis(AnalysisId id)
		{
			_settings.Analyses.Remove(id);
		}

		public bool IsAnalysisActive(AnalysisId id)
		{
			return _settings.Analyses.Contains(id);
		}

		public abstract ILogFile OriginalLogFile { get; }

		public abstract ILogFile UnfilteredLogFile { get; }

		public int NoLevelCount => _counter.NoLevel.LogEntryCount;

		public int TraceCount => _counter.Trace.LogEntryCount;

		public int DebugCount => _counter.Debugs.LogEntryCount;

		public int InfoCount => _counter.Infos.LogEntryCount;

		public int WarningCount => _counter.Warnings.LogEntryCount;

		public int ErrorCount => _counter.Errors.LogEntryCount;

		public int FatalCount => _counter.Fatals.LogEntryCount;

		public int NoTimestampCount => _counter.NoTimestamp.LogEntryCount;

		public string FullFileName => _settings.File;

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Is not persisted on purpose.
		/// </remarks>
		public string CharacterCode { get; set; }

		public bool FollowTail
		{
			get { return _settings.FollowTail; }
			set { _settings.FollowTail = value; }
		}

		public bool ShowLineNumbers
		{
			get { return _settings.ShowLineNumbers; }
			set { _settings.ShowLineNumbers = value; }
		}

		public bool ShowDeltaTimes
		{
			get { return _settings.ShowDeltaTimes; }
			set { _settings.ShowDeltaTimes = value; }
		}
		
		public bool ShowElapsedTime
		{
			get { return _settings.ShowElapsedTime; }
			set { _settings.ShowElapsedTime = value; }
		}

		public HashSet<LogLineIndex> SelectedLogLines
		{
			get { return _settings.SelectedLogLines; }
			set { _settings.SelectedLogLines = value; }
		}

		public LogLineIndex VisibleLogLine
		{
			get { return _settings.VisibleLogLine; }
			set { _settings.VisibleLogLine = value; }
		}

		public double HorizontalOffset
		{
			get { return _settings.HorizontalOffset; }
			set { _settings.HorizontalOffset = value; }
		}

		public DataSource Settings => _settings;

		public int TotalCount => _counter.Total.LogLineCount;

		public Size FileSize => UnfilteredLogFile.GetValue(LogFileProperties.Size) ?? Size.Zero;

		public bool ColorByLevel
		{
			get { return _settings.ColorByLevel; }
			set { _settings.ColorByLevel = value; }
		}

		public bool HideEmptyLines
		{
			get { return _settings.HideEmptyLines; }
			set
			{
				if (value == _settings.HideEmptyLines)
					return;

				_settings.HideEmptyLines = value;
				CreateFilteredLogFile();
			}
		}

		public bool IsSingleLine
		{
			get { return _settings.IsSingleLine; }
			set
			{
				if (value == _settings.IsSingleLine)
					return;

				_settings.IsSingleLine = value;
				OnSingleLineChanged();
			}
		}
		
		public void Dispose()
		{
			_logFile.Dispose();
			_search.Dispose();

			DisposeCurrentSearch();
			_logFile?.Dispose();

			try
			{
				DisposeAdditional();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}

			_isDisposed = true;
		}

		/// <summary>
		///     Called during <see cref="Dispose" />, can be implemented to
		///     dispose of additional resources.
		/// </summary>
		protected abstract void DisposeAdditional();

		public bool IsDisposed => _isDisposed;

		public override string ToString()
		{
			return _settings.ToString();
		}

		/// <summary>
		/// Must be called by suclasses when the <see cref="UnfilteredLogFile"/> property changes
		/// (i.e. returns a different object).
		/// </summary>
		protected void OnUnfilteredLogFileChanged()
		{
			_previousUnfilteredLogFile?.RemoveListener(_counter);
			UnfilteredLogFile.AddListener(_counter, TimeSpan.Zero, 1000);
			_previousUnfilteredLogFile = UnfilteredLogFile;

			CreateFilteredLogFile();
		}

		protected abstract void OnSingleLineChanged();

		private void CreateFilteredLogFile()
		{
			_filteredLogFile?.Dispose();
			
			LevelFlags levelFilter = LevelFilter;
			ILogLineFilter logLineFilter = HideEmptyLines ? (ILogLineFilter)new EmptyLogLineFilter() : new NoFilter();
			ILogEntryFilter logEntryFilter = Filter.Create(levelFilter, _quickFilterChain);
			if (logEntryFilter != null)
			{
				_filteredLogFile = UnfilteredLogFile.AsFiltered(_taskScheduler, logLineFilter, logEntryFilter, _maximumWaitTime);
				_logFile.InnerLogFile = _filteredLogFile;
			}
			else
			{
				_filteredLogFile = null;
				_logFile.InnerLogFile = UnfilteredLogFile;
			}
		}

		private void CreateSearch()
		{
			DisposeCurrentSearch();

			var term = SearchTerm;
			_currentSearch = !string.IsNullOrEmpty(term) ? new LogFileSearch(_taskScheduler, _logFile, term, _maximumWaitTime) : null;
			_search.SearchTerm = SearchTerm;
		}

		private void DisposeCurrentSearch()
		{
			if (_currentSearch != null)
			{
				_currentSearch.Dispose();
				_currentSearch = null;
			}
		}

		private sealed class Counter
			: ICount
		{
			public int LogLineCount { get; set; }
			public int LogEntryCount { get; set; }

			public void Reset()
			{
				LogLineCount = 0;
				LogEntryCount = 0;
			}
		}

		/// <summary>
		///     Responsible for providing the amount of occurences of certain classes of log lines and -entries.
		/// </summary>
		private sealed class LogFileCounter
			: ILogFileListener
		{
			#region Counts

			public readonly Counter Trace;
			public readonly Counter Debugs;
			public readonly Counter Errors;
			public readonly Counter Fatals;
			public readonly Counter Infos;
			public readonly Counter NoLevel;
			public readonly Counter NoTimestamp;
			public readonly Counter Total;
			public readonly Counter Warnings;

			#endregion

			private readonly List<LogLine> _lines;

			public LogFileCounter()
			{
				Fatals = new Counter();
				Errors = new Counter();
				Warnings = new Counter();
				Infos = new Counter();
				Debugs = new Counter();
				Trace = new Counter();
				NoLevel = new Counter();
				NoTimestamp = new Counter();
				Total = new Counter();

				_lines = new List<LogLine>();
			}

			public void OnLogFileModified(ILogFile logFile, LogFileSection section)
			{
				if (section.IsReset)
				{
					Clear();
				}
				else if (section.IsInvalidate)
				{
					RemoveRange(section);
				}
				else
				{
					AddRange(logFile, section);
				}
			}

			private void AddRange(ILogFile logFile, LogFileSection section)
			{
				var previousLine = _lines.Count > 0
					? _lines[_lines.Count - 1]
					: new LogLine(-1, -1, null, LevelFlags.None);

				LogLine[] lines = logFile.GetSection(section);
				for (int i = 0; i < section.Count; ++i)
				{
					LogLine line = lines[i];
					IncrementCount(line, previousLine);
					previousLine = line;
				}
				_lines.AddRange(lines);
			}

			private void RemoveRange(LogFileSection section)
			{
				var previousLine = section.Index > 0
					? _lines[(int) section.Index]
					: new LogLine(-1, -1, null, LevelFlags.None);

				for (int i = 0; i < section.Count; ++i)
				{
					LogLineIndex index = section.Index + i;
					LogLine line = _lines[(int) index];
					DecrementCount(line, previousLine);
					previousLine = line;
				}

				_lines.RemoveRange((int) section.Index, section.Count);
			}

			private void DecrementCount(LogLine currentLogLine, LogLine previousLogLine)
			{
				if (currentLogLine.LogEntryIndex != previousLogLine.LogEntryIndex)
				{
					switch (currentLogLine.Level)
					{
						case LevelFlags.Fatal:
							--Fatals.LogEntryCount;
							break;
						case LevelFlags.Error:
							--Errors.LogEntryCount;
							break;
						case LevelFlags.Warning:
							--Warnings.LogEntryCount;
							break;
						case LevelFlags.Info:
							--Infos.LogEntryCount;
							break;
						case LevelFlags.Debug:
							--Debugs.LogEntryCount;
							break;
						case LevelFlags.Trace:
							--Trace.LogEntryCount;
							break;
						default:
							--NoLevel.LogEntryCount;
							break;
					}

					if (currentLogLine.Timestamp == null)
					{
						--NoTimestamp.LogEntryCount;
					}

					--Total.LogEntryCount;
				}

				switch (currentLogLine.Level)
				{
					case LevelFlags.Fatal:
						--Fatals.LogLineCount;
						break;
					case LevelFlags.Error:
						--Errors.LogLineCount;
						break;
					case LevelFlags.Warning:
						--Warnings.LogLineCount;
						break;
					case LevelFlags.Info:
						--Infos.LogLineCount;
						break;
					case LevelFlags.Debug:
						--Debugs.LogLineCount;
						break;
					case LevelFlags.Trace:
						--Trace.LogLineCount;
						break;
					default:
						--NoLevel.LogLineCount;
						break;
				}

				if (currentLogLine.Timestamp == null)
				{
					--NoTimestamp.LogLineCount;
				}

				--Total.LogLineCount;
			}

			private void IncrementCount(LogLine currentLogLine, LogLine previousLogLine)
			{
				if (currentLogLine.LogEntryIndex != previousLogLine.LogEntryIndex)
				{
					switch (currentLogLine.Level)
					{
						case LevelFlags.Fatal:
							++Fatals.LogEntryCount;
							break;
						case LevelFlags.Error:
							++Errors.LogEntryCount;
							break;
						case LevelFlags.Warning:
							++Warnings.LogEntryCount;
							break;
						case LevelFlags.Info:
							++Infos.LogEntryCount;
							break;
						case LevelFlags.Debug:
							++Debugs.LogEntryCount;
							break;
						case LevelFlags.Trace:
							++Trace.LogEntryCount;
							break;
						default:
							++NoLevel.LogEntryCount;
							break;
					}

					if (currentLogLine.Timestamp == null)
					{
						++NoTimestamp.LogEntryCount;
					}

					++Total.LogEntryCount;
				}

				switch (currentLogLine.Level)
				{
					case LevelFlags.Fatal:
						++Fatals.LogLineCount;
						break;
					case LevelFlags.Error:
						++Errors.LogLineCount;
						break;
					case LevelFlags.Warning:
						++Warnings.LogLineCount;
						break;
					case LevelFlags.Info:
						++Infos.LogLineCount;
						break;
					case LevelFlags.Debug:
						++Debugs.LogLineCount;
						break;
					case LevelFlags.Trace:
						++Trace.LogLineCount;
						break;
					default:
						++NoLevel.LogLineCount;
						break;
				}

				if (currentLogLine.Timestamp == null)
				{
					++NoTimestamp.LogLineCount;
				}

				++Total.LogLineCount;
			}

			private void Clear()
			{
				_lines.Clear();
				Fatals.Reset();
				Errors.Reset();
				Warnings.Reset();
				Infos.Reset();
				Debugs.Reset();
				Trace.Reset();
				NoLevel.Reset();
				NoTimestamp.Reset();
				Total.Reset();
			}
		}
	}
}