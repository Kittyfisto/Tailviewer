using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;
using Tailviewer.Settings.Dashboard.Analysers.Event;

namespace Tailviewer.BusinessLogic.Analysers.Event
{
	/// <summary>
	///     This log analyser is responsible for projecting a source log file
	///     onto an "event" log file based on a set of event definition.
	/// </summary>
	/// <remarks>
	///     If a <see cref="LogLine" /> matches multiple <see cref="LogEventDefinition" />s, then
	///     the first <see cref="LogEventDefinition" /> is responsible for generating the event.
	///     (I.e. regardless of definition, a <see cref="LogLine" /> can only ever be used to create
	///     one LogLine, but not more).
	/// </remarks>
	public sealed class EventsLogAnalyser
		: LogAnalyser
	{
		private const int MaximumLineCount = 10000;

		private readonly LogLine[] _buffer;
		private readonly List<LogEventDefinition> _eventDefinitions;
		private readonly InMemoryLogFile _events;
		private readonly ConcurrentQueue<LogFileSection> _modifications;
		private readonly ITaskScheduler _scheduler;
		private readonly ILogFile _source;
		private readonly IPeriodicTask _task;

		public EventsLogAnalyser(ITaskScheduler scheduler,
			ILogFile source,
			TimeSpan maximumWaitTime,
			EventsAnalyserSettings settings)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			_scheduler = scheduler;
			_source = source;
			_buffer = new LogLine[MaximumLineCount];
			_events = new InMemoryLogFile();
			_eventDefinitions = new List<LogEventDefinition>(settings.Events.Count);
			_eventDefinitions.AddRange(settings.Events.Select(x => new LogEventDefinition(x)));
			_modifications = new ConcurrentQueue<LogFileSection>();

			_source.AddListener(this, maximumWaitTime, MaximumLineCount);
			_task = _scheduler.StartPeriodic(DoWork, TimeSpan.FromMilliseconds(100));
		}

		/// <summary>
		///     The events produced by this analyser.
		/// </summary>
		public ILogFile Events => _events;

		private void DoWork()
		{
			try
			{
				StartMeasure();

				LogFileSection modification;
				while (_modifications.TryDequeue(out modification))
					if (modification.IsReset)
						_events.Clear();
					else if (modification.IsInvalidate)
						_events.RemoveFrom(modification.Index);
					else
						TryExtractEventsFrom(modification);
			}
			finally
			{
				StopMeasure();
			}
		}

		private void TryExtractEventsFrom(LogFileSection modification)
		{
			try
			{
				ExtractEventsFrom(modification);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private void ExtractEventsFrom(LogFileSection modification)
		{
			_source.GetSection(modification, _buffer);
			ExtractEventsFrom(_buffer, modification.Count);
		}

		private void ExtractEventsFrom(LogLine[] logLines, int count)
		{
			for (var i = 0; i < count; ++i)
			{
				var logLine = logLines[i];
				var @event = TryExtractEventFrom(logLine);
				if (@event != null)
					_events.AddEntry(@event, logLine.Level, logLine.Timestamp);
			}
		}

		private string TryExtractEventFrom(LogLine logLine)
		{
			foreach (var eventDefinition in _eventDefinitions)
			{
				var @event = eventDefinition.TryExtractEventFrom(logLine);
				if (@event != null)
					return @event;
			}

			return null;
		}

		protected override void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section)
		{
			_modifications.Enqueue(section);
		}

		protected override void OnLogTableModifiedInternal(ILogTable logTable, LogTableModification modification)
		{
		}

		protected override void DisposeInternal()
		{
			_scheduler.StopPeriodic(_task);
			_source.RemoveListener(this);
		}
	}
}