using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace Tailviewer.BusinessLogic.Scheduling
{
	internal sealed class TaskQueue
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly HashSet<PeriodicTask> _tasks;
		private readonly List<PeriodicTask> _pendingTasks;
		private readonly object _syncRoot;

		public TaskQueue()
		{
			_syncRoot = new object();
			_tasks = new HashSet<PeriodicTask>();
			_pendingTasks = new List<PeriodicTask>();
		}

		public void Add(PeriodicTask task)
		{
			if (task == null)
				throw new ArgumentNullException("task");

			lock (_syncRoot)
			{
				if (task.IsRemoved)
				{
					if (Log.IsDebugEnabled)
					{
						Log.DebugFormat("Periodic task '{0}' has been removed and will no longer be added to the qeueue",
										task);
					}
				}
				else
				{
					if (Log.IsDebugEnabled)
					{
						Log.DebugFormat("Periodic task '{0}' has finished executing and is added to the task queue once more");
					}

					if (_tasks.Add(task))
					{
						_pendingTasks.Add(task);
					}
				}
			}
		}

		public void EnqueuePending(PeriodicTask task)
		{
			if (task == null)
				throw new ArgumentNullException("task");

			lock (_syncRoot)
			{
				if (task.IsRemoved)
				{
					if (Log.IsDebugEnabled)
					{
						Log.DebugFormat("Periodic task '{0}' has been removed and will no longer be added to the qeueue",
										task);
					}
				}
				else
				{
					if (Log.IsDebugEnabled)
					{
						Log.DebugFormat("Periodic task '{0}' has finished executing and is added to the task queue once more");
					}

					if (!_pendingTasks.Contains(task))
						_pendingTasks.Add(task);
				}
			}
		}

		public bool Remove(PeriodicTask task)
		{
			lock (_syncRoot)
			{
				if (_tasks.Remove(task))
				{
					// We don't know if this task is currently pending
					// or being executed or about to be.
					// Therefore we set the removed flag and then try to remove
					// it from the queue...
					task.IsRemoved = true;
					_pendingTasks.Remove(task);

					return true;
				}

				return false;
			}
		}

		public bool TryDequeuePendingTask(DateTime now, out PeriodicTask task, out TimeSpan remainingMinimumWaitTime)
		{
			remainingMinimumWaitTime = TimeSpan.MaxValue;

			lock (_syncRoot)
			{
				for (int i = 0; i < _pendingTasks.Count; ++i)
				{
					var possibleTask = _pendingTasks[i];

					TimeSpan remaining;
					if (possibleTask.ShouldRun(now, out remaining))
					{
						_pendingTasks.RemoveAt(i);
						task = possibleTask;
						remainingMinimumWaitTime = TimeSpan.Zero;
						return true;
					}

					if (remaining < remainingMinimumWaitTime)
						remainingMinimumWaitTime = remaining;
				}
			}

			task = null;
			return false;
		}
	}
}