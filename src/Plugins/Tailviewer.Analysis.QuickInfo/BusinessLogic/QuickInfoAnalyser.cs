using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.Filters;

namespace Tailviewer.Analysis.QuickInfo.BusinessLogic
{
	public sealed class QuickInfoAnalyser
		: AbstractLogAnalyser
	{
		private const int MaximumLineCount = 10000;
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<Guid, LogLine?> _lastMatchingLines;
		private readonly IReadOnlyDictionary<ILogFile, Guid> _logFiles;
		private readonly ConcurrentQueue<PendingChange> _pendingChanges;

		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;
		private QuickInfoResult _currentResult;

		public QuickInfoAnalyser(IServiceContainer services,
		                         ILogFile source,
		                         TimeSpan maximumWaitTime,
		                         QuickInfoAnalyserConfiguration configuration)
			: base(services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			_scheduler = services.Retrieve<ITaskScheduler>();
			_pendingChanges = new ConcurrentQueue<PendingChange>();
			_lastMatchingLines = new Dictionary<Guid, LogLine?>();
			var logFiles = new Dictionary<ILogFile, Guid>();
			_logFiles = logFiles;
			try
			{
				foreach (var config in configuration.QuickInfos)
				{
					var filter = CreateFilter(config);
					var filteredLogFile = services.CreateFilteredLogFile(maximumWaitTime, source, filter);
					logFiles.Add(filteredLogFile, config.Id);
					_lastMatchingLines.Add(config.Id, value: null);
					filteredLogFile.AddListener(this, maximumWaitTime, MaximumLineCount);
				}
			}
			catch (Exception)
			{
				Dispose(logFiles.Keys);
				throw;
			}

			_task = _scheduler.StartPeriodic(OnUpdate, maximumWaitTime, "Quick Info Analyser");
		}

		public override ILogAnalysisResult Result => _currentResult;

		public override Percentage Progress => Percentage.Zero;

		private void OnUpdate()
		{
			var numChanges = 0;
			PendingChange change;
			while (_pendingChanges.TryDequeue(out change))
			{
				Guid quickInfoId;
				if (_logFiles.TryGetValue(change.LogFile, out quickInfoId))
				{
					LogLine? previousLine;
					_lastMatchingLines.TryGetValue(quickInfoId, out previousLine);

					var matchingLine = GetMatchingLine(change.LogFile, change.Section, previousLine);
					_lastMatchingLines[quickInfoId] = matchingLine;
				}

				++numChanges;
				if (numChanges % 20 == 0)
					_currentResult = CreateResult(_lastMatchingLines);
			}

			if (numChanges > 0)
				_currentResult = CreateResult(_lastMatchingLines);
		}

		/// <summary>
		///     Creates a new result from the given currently matched lines.
		/// </summary>
		/// <remarks>
		///     Only those results where a line has been found will be part of the returned result.
		/// </remarks>
		/// <param name="lastMatchingLines"></param>
		/// <returns></returns>
		private static QuickInfoResult CreateResult(IReadOnlyDictionary<Guid, LogLine?> lastMatchingLines)
		{
			var result = new QuickInfoResult(lastMatchingLines.Count);
			foreach (var pair in lastMatchingLines)
			{
				var quickInfo = CreateResult(pair.Value);
				if (quickInfo != null)
					result.QuickInfos.Add(pair.Key, quickInfo);
			}
			return result;
		}

		private static QuickInfo CreateResult(LogLine? line)
		{
			if (line == null)
				return null;

			return new QuickInfo(line.Value.Message, line.Value.Timestamp);
		}

		/// <summary>
		///     Determines the next matching log line given the previously matched line (if any)
		///     and current change in filtered log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="change"></param>
		/// <param name="previousLine"></param>
		/// <returns></returns>
		private static LogLine? GetMatchingLine(ILogFile logFile, LogFileSection change, LogLine? previousLine)
		{
			if (change.IsReset)
				return null;

			if (change.IsInvalidate)
			{
				if (previousLine != null)
				{
					if (previousLine.Value.LineIndex >= change.Index)
						return null;

					// The previous matching line is still valid
					return previousLine;
				}

				// We didn't have a matching line before and an invalidation doesn't change that
				return null;
			}

			// We wanna fetch the last matching line:
			try
			{
				var last = change.LastIndex;
				var line = logFile.GetLine(last);
				return line;
			}
			catch (Exception e)
			{
				// My hope is that this exception only occurs when the section
				// has been invalidated and therefore there should be another pending
				// change to be processed.
				Log.DebugFormat("Caught exception while retrieving matching line: {0}", e);

				return null;
			}
		}

		private static ILogEntryFilter CreateFilter(QuickInfoConfiguration configuration)
		{
			var filter = Filter.Create(configuration.FilterValue,
				configuration.MatchType,
				ignoreCase: true,
				isInverted: false);
			return filter;
		}

		protected override void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section)
		{
			Guid quickInfoId;
			if (_logFiles.TryGetValue(logFile, out quickInfoId))
				_pendingChanges.Enqueue(new PendingChange(logFile, section));
		}

		protected override void DisposeInternal()
		{
			_scheduler.StopPeriodic(_task);
			Dispose(_logFiles.Keys);
		}

		private static void Dispose(IEnumerable<IDisposable> values)
		{
			foreach (var value in values)
				try
				{
					value.Dispose();
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception: {0}", e);
				}
		}

		private struct PendingChange
		{
			public readonly ILogFile LogFile;
			public readonly LogFileSection Section;

			public PendingChange(ILogFile logFile, LogFileSection section)
			{
				LogFile = logFile;
				Section = section;
			}
		}
	}
}