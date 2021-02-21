using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.Core.Buffers;

namespace Tailviewer.Core.Sources
{
	internal abstract class AbstractProxyLogSource
		: AbstractLogSource
		, ILogSourceListener
	{
		private readonly LogBufferArray _fetchBuffer;
		private readonly ConcurrentQueue<LogFileSection> _pendingSections;
		private readonly ILogSource _source;
		private readonly TimeSpan _maximumWaitTime;
		private int _count;

		protected AbstractProxyLogSource(ITaskScheduler taskScheduler,
		                                 ILogSource source,
										 IReadOnlyList<IColumnDescriptor> columnsToFetch,
										 TimeSpan maximumWaitTime,
		                                 int maxEntryCount = 1000)
			: base(taskScheduler)
		{
			_fetchBuffer = new LogBufferArray(maxEntryCount, columnsToFetch);
			_pendingSections = new ConcurrentQueue<LogFileSection>();
			_source = source;
			_maximumWaitTime = maximumWaitTime;

			_source.AddListener(this, maximumWaitTime, maxEntryCount);

			StartTask();
		}

		#region Overrides of AbstractLogSource

		protected override void DisposeAdditional()
		{
			_source.RemoveListener(this);

			base.DisposeAdditional();
		}

		#endregion

		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogFileSection section)
		{
			_pendingSections.Enqueue(section);
		}

		#endregion

		#region Overrides of AbstractLogSource

		protected sealed override TimeSpan RunOnce(CancellationToken token)
		{
			var pendingSections = _pendingSections.DequeueAll();
			if (!Process(pendingSections))
			{
				NothingToProcess();
				Listeners.OnRead(_count);
			}

			return _maximumWaitTime;
		}

		#endregion
		private bool Process(IReadOnlyList<LogFileSection> pendingSections)
		{
			if (pendingSections.Count == 0)
				return false;

			foreach (var section in pendingSections)
			{
				if (section.IsReset)
				{
					_count = 0;
					Listeners.Reset();
					OnReset();
				}
				else if (section.IsInvalidate)
				{
					_count = (int) section.Index;
					OnInvalidate(_count);
					Listeners.Invalidate((int) section.Index, section.Count);
				}
				else
				{
					_count += section.Count;
					_source.GetEntries(section, _fetchBuffer);
					OnAdd(section, _fetchBuffer, _count);
					Listeners.OnRead(_count);
				}
			}

			return true;
		}

		protected abstract void OnReset();

		protected abstract void OnInvalidate(int totalLogEntryCount);

		protected abstract void OnAdd(LogFileSection section, IReadOnlyLogBuffer data, int totalLogEntryCount);

		protected abstract void NothingToProcess();
	}
}