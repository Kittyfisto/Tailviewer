using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	internal abstract class AbstractDataSource
		: IDataSource
	{
		private readonly LogFileCounter _counter;
		private readonly TimeSpan _maximumWaitTime;
		private readonly DataSource _settings;
		private ILogFile _filteredLogFile;
		private ILogFile _lastRegisteredLogFile;
		private IEnumerable<ILogEntryFilter> _quickFilterChain;

		protected AbstractDataSource(DataSource settings, TimeSpan maximumWaitTime)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (settings.Id == Guid.Empty) throw new ArgumentException("settings.Id shall be set to an actually generated id");

			_settings = settings;
			_maximumWaitTime = maximumWaitTime;
			_counter = new LogFileCounter();
		}

		public ILogFile FilteredLogFile
		{
			get { return _filteredLogFile; }
		}

		/// <summary>
		///     The list of filters as produced by the "quick filter" panel.
		/// </summary>
		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _quickFilterChain; }
			set
			{
				if (value == _quickFilterChain)
					return;

				_quickFilterChain = value;
				CreateFilteredLogFile();
			}
		}

		public string StringFilter
		{
			get { return _settings.StringFilter; }
			set
			{
				if (value == StringFilter)
					return;

				_settings.StringFilter = value;
				CreateFilteredLogFile();
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

		public DateTime LastModified
		{
			get { return LogFile.LastModified; }
		}

		public DateTime LastViewed
		{
			get { return _settings.LastViewed; }
			set { _settings.LastViewed = value; }
		}

		public Guid Id
		{
			get { return _settings.Id; }
		}

		public Guid ParentId
		{
			get { return _settings.ParentId; }
		}

		public void ActivateQuickFilter(Guid id)
		{
			// Should I add a sanity check here?
			_settings.ActivatedQuickFilters.Add(id);
		}

		public bool DeactivateQuickFilter(Guid id)
		{
			return _settings.ActivatedQuickFilters.Remove(id);
		}

		public bool IsQuickFilterActive(Guid id)
		{
			return _settings.ActivatedQuickFilters.Contains(id);
		}

		public abstract ILogFile LogFile { get; }

		public int NoLevelCount
		{
			get { return _counter.NoLevel.LogEntryCount; }
		}

		public int DebugCount
		{
			get { return _counter.Debugs.LogEntryCount; }
		}

		public int InfoCount
		{
			get { return _counter.Infos.LogEntryCount; }
		}

		public int WarningCount
		{
			get { return _counter.Warnings.LogEntryCount; }
		}

		public int ErrorCount
		{
			get { return _counter.Errors.LogEntryCount; }
		}

		public int FatalCount
		{
			get { return _counter.Fatals.LogEntryCount; }
		}

		public int NoTimestampCount
		{
			get { return _counter.NoTimestamp.LogEntryCount; }
		}

		public string FullFileName
		{
			get { return _settings.File; }
		}

		public bool FollowTail
		{
			get { return _settings.FollowTail; }
			set { _settings.FollowTail = value; }
		}

		public LogLineIndex SelectedLogLine
		{
			get { return _settings.SelectedLogLine; }
			set { _settings.SelectedLogLine = value; }
		}

		public LogLineIndex VisibleLogLine
		{
			get { return _settings.VisibleLogLine; }
			set { _settings.VisibleLogLine = value; }
		}

		public DataSource Settings
		{
			get { return _settings; }
		}

		public int TotalCount
		{
			get { return _counter.Total.LogLineCount; }
		}

		public Size FileSize
		{
			get { return LogFile.FileSize; }
		}

		public bool ColorByLevel
		{
			get { return _settings.ColorByLevel; }
			set { _settings.ColorByLevel = value; }
		}

		public void Dispose()
		{
			LogFile.Dispose();
		}

		public override string ToString()
		{
			return _settings.ToString();
		}

		protected void CreateFilteredLogFile()
		{
			if (LogFile != _lastRegisteredLogFile)
			{
				// Our counter doesn't really do anything so it can be called instantly...
				LogFile.AddListener(_counter, TimeSpan.Zero, 1000);
				_lastRegisteredLogFile = LogFile;
			}

			string stringFilter = StringFilter;
			LevelFlags levelFilter = LevelFilter;

			ILogFile newLogFile;
			ILogEntryFilter filter = Filter.Create(stringFilter, true, levelFilter, _quickFilterChain);
			if (filter != null)
			{
				newLogFile = LogFile.AsFiltered(filter, _maximumWaitTime);
			}
			else
			{
				newLogFile = LogFile;
			}

			if (_filteredLogFile != null)
			{
				if (_filteredLogFile != LogFile)
					_filteredLogFile.Dispose();
			}

			_filteredLogFile = newLogFile;
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
				else if (section.InvalidateSection)
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
				LogLine previousLine;
				if (_lines.Count > 0)
					previousLine = _lines[_lines.Count - 1];
				else
					previousLine = new LogLine(-1, -1, null, LevelFlags.None);

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
				LogLine previousLine;
				if (section.Index > 0)
					previousLine = _lines[(int) section.Index];
				else
					previousLine = new LogLine(-1, -1, null, LevelFlags.None);

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
				NoLevel.Reset();
				NoTimestamp.Reset();
				Total.Reset();
			}
		}
	}
}