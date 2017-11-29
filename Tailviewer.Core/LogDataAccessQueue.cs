using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogTables;

namespace Tailviewer.Core
{
	/// <summary>
	///     This class is responsible for serializing asynchronous access a some data source.
	///     Is meant to be used by implementations of <see cref="ILogFile" /> and <see cref="ILogTable" />.
	/// </summary>
	/// <remarks>
	/// TODO: This class should be able to spot access patterns and then retrieve multiple values with just one call (for example by requesting a range)
	/// </remarks>
	internal sealed class LogDataAccessQueue<TIndex, TData>
		: IDisposable
		where TIndex : struct, IEquatable<TIndex>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Queue<TaskData> _pendingRequests;
		private readonly Dictionary<TIndex, TaskData> _requestsByIndex;
		private readonly object _syncRoot;

		public LogDataAccessQueue()
		{
			_syncRoot = new object();
			_pendingRequests = new Queue<TaskData>();
			_requestsByIndex = new Dictionary<TIndex, TaskData>();
		}

		public ITask<TData> this[TIndex index]
		{
			get
			{
				TaskData data;
				lock (_syncRoot)
				{
					// We want to avoid retrieving data of the same index multiple times.
					// Therefore if a request to the same index is already pending, then
					// we re-use it instead of adding a 2nd requests. This will be good enough
					// to deal with "bursts" to the same index, however in order to improve
					// performance, users should add a 2nd layer of caches that keep data in
					// memory for a longer amount of time.
					if (!_requestsByIndex.TryGetValue(index, out data))
					{
						data = new TaskData(index);
						_pendingRequests.Enqueue(data);
						_requestsByIndex.Add(index, data);
					}
				}
				return data.Task;
			}
		}

		public int Count
		{
			get
			{
				lock (_syncRoot)
				{
					return _pendingRequests.Count;
				}
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				TaskData data;
				while (TryDequeueNoLock(out data))
				{
					data.Cancel();
				}
			}
		}

		/// <summary>
		///     Tries to dequeue a pending task.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private bool TryDequeue(out TaskData data)
		{
			lock (_syncRoot)
			{
				return TryDequeueNoLock(out data);
			}
		}

		/// <summary>
		///     Tries to dequeue a pending task without taking a lock.
		///     CALLERS MUST MAKE SURE TO HAVE LOCKED THE SYNC ROOT!
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private bool TryDequeueNoLock(out TaskData data)
		{
			if (_pendingRequests.TryDequeue(out data))
			{
				if (!_requestsByIndex.Remove(data.Index))
				{
					Log.WarnFormat(
						"Inconsistency detected: We were able to remove a pending task from the queue, but not from the dictionary. This suggests an undetected race condition!");
				}

				return true;
			}

			return false;
		}

		public void ExecuteAll(ILogDataAccessor<TIndex, TData> accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException(nameof(accessor));

			TaskData data;
			while (TryDequeue(out data))
			{
				// DO NOT PLACE ANY CODE BETWEEN TryDequeue and Execute, EVER
				Execute(data, accessor);
			}
		}

		public void ExecuteOne(ILogDataAccessor<TIndex, TData> accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException(nameof(accessor));

			TaskData data;
			if (TryDequeue(out data))
			{
				// DO NOT PLACE ANY CODE BETWEEN TryDequeue and Execute, EVER
				Execute(data, accessor);
			}
		}

		private void Execute(TaskData data, ILogDataAccessor<TIndex, TData> accessor)
		{
			try
			{
				TData value;
				if (accessor.TryAccess(data.Index, out value))
				{
					data.Finished(value);
				}
				else
				{
					// If this region of data can no longer be accessed,
					// then we cancel the request.
					data.Cancel();
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception");
				data.Failed(e);
			}
		}

		private struct TaskData
		{
			public readonly TIndex Index;
			private readonly NoThrowTaskCompletionSource<TData> _completionSource;

			public TaskData(TIndex index)
			{
				Index = index;
				_completionSource = new NoThrowTaskCompletionSource<TData>();
			}

			public ITask<TData> Task
			{
				get { return _completionSource.Task; }
			}

			public void Finished(TData value)
			{
				if (!_completionSource.TrySetResult(value))
				{
					Log.ErrorFormat("Unable to set the result for index {0}: {1}", Index, value);
				}
			}

			public void Failed(Exception e)
			{
				if (!_completionSource.TrySetException(e))
				{
					Log.ErrorFormat("Unable to set the exception for index {0}: {1}", Index, e);
				}
			}

			public void Cancel()
			{
				if (!_completionSource.TrySetCanceled())
				{
					Log.ErrorFormat("Unable to cancel the index {0}", Index);
				}
			}
		}
	}
}