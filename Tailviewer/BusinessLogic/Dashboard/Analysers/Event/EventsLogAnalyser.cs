using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;
using Tailviewer.Settings.Dashboard.Analysers.Event;

namespace Tailviewer.BusinessLogic.Dashboard.Analysers.Event
{
	/// <summary>
	/// This log analyser is responsible for projecting a source log file
	/// onto an "event" log file based on a 
	/// </summary>
	public sealed class EventsLogAnalyser
		: LogAnalyser
	{
		private readonly List<LogEventDefinition> _eventDefinitions;
		private readonly InMemoryLogFile _events;
		private readonly ConcurrentQueue<LogFileSection> _modifications;
		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;

		public EventsLogAnalyser(ITaskScheduler scheduler, EventsAnalyserSettings settings)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			_scheduler = scheduler;
			_eventDefinitions = new List<LogEventDefinition>(settings.Events.Count);
			_eventDefinitions.AddRange(settings.Events.Select(x => new LogEventDefinition(x)));
			_modifications = new ConcurrentQueue<LogFileSection>();

			_task = _scheduler.StartPeriodic(DoWork, TimeSpan.FromMilliseconds(100));
		}

		private void DoWork()
		{
			LogFileSection modification;
			while (_modifications.TryDequeue(out modification))
			{
				if (modification.IsReset)
				{
					// Start all over...
					_events.Clear();
				}
				else if (modification.IsInvalidate)
				{
					_events.RemoveRange(modification.Index, modification.Count);
				}
				else
				{
					
				}
			}
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
		}
	}
}