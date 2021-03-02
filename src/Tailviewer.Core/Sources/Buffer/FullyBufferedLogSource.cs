using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Buffers the entire underlying source in memory.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class FullyBufferedLogSource
		: AbstractProcessingLogSource
	{
		private readonly LogBufferList _buffer;
		private readonly object _syncRoot;

		public FullyBufferedLogSource(ITaskScheduler taskScheduler,
		                              ILogSource source)
			: this(taskScheduler, source, source.Columns, TimeSpan.FromMilliseconds(value: 100))
		{}

		public FullyBufferedLogSource(ITaskScheduler taskScheduler,
		                              ILogSource source,
		                              IReadOnlyList<IColumnDescriptor> bufferedColumns,
		                              TimeSpan maximumWaitTime)
			: base(taskScheduler,
			       source,
			       bufferedColumns,
			       new IReadOnlyPropertyDescriptor[0],
			       maximumWaitTime)
		{
			_buffer = new LogBufferList(bufferedColumns);
			_syncRoot = new object();

			StartTask();
		}

		#region Overrides of AbstractLogSource

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                                  IColumnDescriptor<T> column,
		                                  T[] destination,
		                                  int destinationIndex,
		                                  LogSourceQueryOptions queryOptions)
		{
			lock (_syncRoot)
			{
				if (Equals(column, PageBufferedLogSource.RetrievalState))
				{
					var dest = (RetrievalState[]) (object) destination;
					if (sourceIndices is LogSourceSection section)
					{
						var totalCount = (int)(section.Index + section.Count);
						var fillCount = Math.Min(totalCount, _buffer.Count);
						dest.Fill(RetrievalState.Retrieved, destinationIndex, fillCount);
						if (totalCount > fillCount)
						{
							dest.Fill(RetrievalState.NotInSource, fillCount, totalCount - fillCount);
						}
					}
					else
					{
						for(int i = 0; i < sourceIndices.Count; ++i)
						{
							var index = sourceIndices[i];
							dest[destinationIndex + i] = index < _buffer.Count
								? RetrievalState.Retrieved
								: RetrievalState.NotInSource;
						}
					}
				}
				else
				{
					_buffer.CopyTo(column, new Int32View(sourceIndices), destination, destinationIndex);
				}
			}
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogBuffer destination,
		                                int destinationIndex,
		                                LogSourceQueryOptions queryOptions)
		{
			lock (_syncRoot)
			{
				_buffer.CopyTo(new Int32View(sourceIndices), destination, destinationIndex);
			}
		}

		protected override void OnResetSection()
		{
			lock (_syncRoot)
			{
				_buffer.Clear();
			}
		}

		protected override void OnSectionRemoved(int totalCount)
		{
			lock (_syncRoot)
			{
				_buffer.RemoveRange(totalCount, _buffer.Count - totalCount);
			}
		}

		protected override void OnSectionAppended(LogSourceSection section, IReadOnlyLogBuffer data, int totalLogEntryCount)
		{
			lock (_syncRoot)
			{
				_buffer.AddRange(data, section.Count);
			}
		}

		protected override void NothingToProcess()
		{
		}

		protected override void GetOverwrittenProperties(PropertiesBufferList destination)
		{
		}

		#endregion
	}
}