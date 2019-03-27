using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public sealed class LogLineCache
		: ILogLineCache
	{
		private const int RequestCount = 100;
		private readonly ILogLineAccessor _accessor;
		private readonly Dictionary<LogLineIndex, LogLine> _cachedLines;
		private readonly Size _maximumSize;
		private readonly ISerialTaskScheduler _scheduler;
		private readonly object _syncRoot;
		private Size _size;

		public LogLineCache(ISerialTaskScheduler scheduler, ILogLineAccessor accessor, Size maximumSize)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (accessor == null)
				throw new ArgumentNullException(nameof(accessor));

			_scheduler = scheduler;
			_accessor = accessor;
			_maximumSize = maximumSize;
			_cachedLines = new Dictionary<LogLineIndex, LogLine>();
			_syncRoot = new object();
		}

		public Size EstimatedSize => _size;

		public Task<LogLineResponse> RequestAsync(LogFileSection section, CancellationToken cancellationToken)
		{
			return _scheduler.StartNew(() => Request(section));
		}

		private LogLineResponse Request(LogFileSection section)
		{
			var lines = new LogLine[section.Count];

			// TODO: Can we retrieve those lines with fewer locks?
			for (var i = 0; i < section.Count; ++i)
			{
				var index = section.Index + i;
				LogLine line;
				if (TryRetrieveFromCache(index, out line))
					lines[i] = line;
				else
					lines[i] = RetrieveFromAccessor(i);
			}

			return new LogLineResponse
			{
				Lines = lines
			};
		}

		public void Invalidate(LogFileSection section)
		{
			lock (_syncRoot)
			{
				for (var i = 0; i < section.Count; ++i)
				{
					var index = section.Index + i;
					_cachedLines.Remove(index);
				}

				EstimateSize();
			}
		}

		private void EstimateSize()
		{
			throw new NotImplementedException();
		}

		private bool TryRetrieveFromCache(LogLineIndex index, out LogLine line)
		{
			lock (_syncRoot)
			{
				return _cachedLines.TryGetValue(index, out line);
			}
		}

		/// <summary>
		///     Purges lines from this cache so the memory used is less than <see cref="_maximumSize" />.
		/// </summary>
		/// <returns></returns>
		private bool Purge()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Retrieves the log line with the given index from this cache's accessor.
		///     Blocks until the accessor returns the line...
		/// </summary>
		/// <remarks>
		///     Returns an empty line if the given index is no longer valid.
		/// </remarks>
		/// <param name="desiredIndex"></param>
		/// <returns></returns>
		private LogLine RetrieveFromAccessor(int desiredIndex)
		{
			var startIndex = desiredIndex / RequestCount;
			var section = new LogFileSection(startIndex, RequestCount);
			var response = _accessor.Request(section);
			var desiredLine = new LogLine();

			lock (_syncRoot)
			{
				for (var i = 0; i < response.ActualSection.Count; ++i)
				{
					var index = response.ActualSection.Index + i;
					var line = response.Lines[i];

					if (i == desiredIndex)
						desiredLine = line;

					_cachedLines.Add(index, line);
				}
			}

			return desiredLine;
		}
	}
}