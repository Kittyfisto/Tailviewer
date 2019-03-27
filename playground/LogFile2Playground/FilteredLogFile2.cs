using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public sealed class FilteredLogFile2
		: ILogFile2
	{
		private readonly ILogFile2 _source;
		private readonly List<LogLineIndex> _indices;
		private readonly object _syncRoot;

		public FilteredLogFile2(ILogFile2 source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			_source = source;
			_syncRoot = new object();
			_indices = new List<LogLineIndex>();
		}

		public Task<LogLineResponse> RequestAsync(LogFileSection section)
		{
			return RequestAsync(section, new CancellationToken(false));
		}

		public Task<LogLineResponse> RequestAsync(LogFileSection section, CancellationToken cancellationToken)
		{
			// We have all the indices in memory and thus we can retrieve the
			// list of original indices immediately because it's a very inexpensive operation.
			var originalIndices = new LogLineIndex[section.Count];
			var tasks = new Task<LogLineResponse>[section.Count];

			lock (_syncRoot)
			{
				// !!!! DON'T GIVE CONTROL TO ANYONE ELSE INSIDE THIS LOCK !!!!
				for (int i = 0; i < section.Count; ++i)
				{
					var filteredIndex = section.Index + i;
					var originalIndex = _indices[filteredIndex.Value];
					originalIndices[i] = originalIndex;
				}
			}

			for (int i = 0; i < section.Count; ++i)
			{
				tasks[i] = _source.RequestAsync(new LogFileSection(originalIndices[i], 1), cancellationToken);
			}

			return Task.WhenAll(tasks).ContinueWith(unused =>
			{
				var lines = new LogLine[section.Count];
				// The caller may have requested a (partially) invalid section, for example because those parts
				// of the log file have been removed (due to filtering, etc...). It is for this reason that
				// we allow partial responses which consists of the first N successful responses.
				int actualCount;
				for (actualCount = 0; actualCount < section.Count && !tasks[actualCount].IsCanceled && !tasks[actualCount].IsFaulted; ++actualCount)
				{
					var task = tasks[actualCount];
					if (task.IsCompleted)
					{
						var result = tasks[actualCount].Result;
						lines[actualCount] = result.Lines[0];
					}
				}
				var responses = new LogLineResponse
				{
					Lines = lines,
					ActualSection = new LogFileSection(section.Index, actualCount)
				};
				return responses;
			}, cancellationToken);
		}
	}
}