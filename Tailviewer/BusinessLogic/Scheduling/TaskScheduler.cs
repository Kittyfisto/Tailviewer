using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tailviewer.BusinessLogic.Scheduling
{
	/// <summary>
	///     Similar to <see cref="System.Threading.Tasks.TaskScheduler" />, it is capable of scheduling tasks.
	///     Can also schedule periodic tasks that are executed with a minimum time between them (until removed).
	///     <see cref="StartPeriodic" /> and <see cref="RemovePeriodic" />.
	/// </summary>
	public sealed class TaskScheduler
		: ITaskScheduler
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ManualResetEvent _disposed;

		private readonly System.Threading.Tasks.TaskScheduler _scheduler;
		private readonly Thread _schedulingThread;
		private readonly ManualResetEvent _taskAdded;
		private readonly TaskQueue _periodicTaskQueue;
		private long _lastTaskId;

		public TaskScheduler()
			: this(System.Threading.Tasks.TaskScheduler.Default)
		{
		}

		private TaskScheduler(System.Threading.Tasks.TaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");

			_scheduler = scheduler;
			_periodicTaskQueue = new TaskQueue();
			_taskAdded = new ManualResetEvent(false);
			_disposed = new ManualResetEvent(false);
			_schedulingThread = new Thread(SchedulePeriodicTasks)
				{
					Name = "Periodic Task Scheduler",
					IsBackground = true,
				};
			_schedulingThread.Start();
		}

		public void Dispose()
		{
			_disposed.Set();
			// There's really no benefit to waiting until the thread has finished
			// executing, therefore we return immediately...
		}

		public int PeriodicTaskCount
		{
			get { return _periodicTaskQueue.Count; }
		}

		/// <summary>
		///     Creates and starts a new task.
		///     The given <paramref name="callback" /> will be executed exactly once.
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		public Task Start(Action callback)
		{
			var task = new Task(callback);
			task.Start(_scheduler);
			return task;
		}

		/// <summary>
		///     Creates and starts a new task.
		///     The given <paramref name="callback" /> will be executed exactly once.
		///     <see cref="Task{T}.Result" /> will carry the return value of the given callback, once its available.
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		public Task<T> Start<T>(Func<T> callback)
		{
			var task = new Task<T>(callback);
			task.Start(_scheduler);
			return task;
		}

		/// <summary>
		///     Creates and starts a new periodic task.
		///     Periodic tasks (as the name suggests) are tasks that are periodically invoked (instead of just once).
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="minimumWaitTime">
		///     The minimum time that should ellapsed between two successive calls to <paramref name="callback" />
		/// </param>
		/// <param name="name">The name of the task, for debugging purposes</param>
		/// <returns></returns>
		public IPeriodicTask StartPeriodic(Action callback, TimeSpan minimumWaitTime, string name = null)
		{
			long id = Interlocked.Increment(ref _lastTaskId);
			var task = new PeriodicTask(id, callback, minimumWaitTime, name);

			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Starting periodic task {0} at {1}ms intervals", task, minimumWaitTime.TotalMilliseconds);
			}

			_periodicTaskQueue.Add(task);
			_taskAdded.Set();
			return task;
		}

		/// <summary>
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public bool RemovePeriodic(IPeriodicTask task)
		{
			var actualTask = task as PeriodicTask;
			if (actualTask == null)
				return false;

			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Removing periodic task {0}", task);
			}

			return _periodicTaskQueue.Remove(actualTask);
		}

		private void SchedulePeriodicTasks()
		{
			while (true)
			{
				try
				{
					SchedulePendingTasksOnce();
				}
				catch (OperationCanceledException e)
				{
					Log.DebugFormat("Scheduling was cancelled because this scheduler has been disposed of, shutting down: {0}", e);
					break;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception: {0}", e);
				}
			}
		}

		/// <summary>
		///     Schedules all pending, elligible tasks exactly once.
		/// </summary>
		private void SchedulePendingTasksOnce()
		{
			TimeSpan remainingMinimumWaitTime;
			int numTasksRun = RunTasksIfPossible(out remainingMinimumWaitTime);
			if (numTasksRun == 0)
			{
				if (Log.IsDebugEnabled)
				{
					Log.DebugFormat("Waiting {0}ms before the next task is scheduled", remainingMinimumWaitTime.TotalMilliseconds);
				}

				if (remainingMinimumWaitTime == TimeSpan.MaxValue)
					remainingMinimumWaitTime = TimeSpan.FromMinutes(1);

				var events = new WaitHandle[]
					{
						_taskAdded,
						_disposed
					};
				int ret = WaitHandle.WaitAny(events, remainingMinimumWaitTime);
				if (ret == 0)
				{
					if (Log.IsDebugEnabled)
					{
						Log.DebugFormat("Waiting '{0}ms' interrupted because a new periodic task was scheduled",
						                remainingMinimumWaitTime.TotalMilliseconds);
					}
				}
				else if (ret == 1)
				{
					throw new OperationCanceledException();
				}
				else if (ret == -1)
				{
					if (Log.IsDebugEnabled)
					{
						Log.DebugFormat("Waited '{0}ms', a new task should be ready now", remainingMinimumWaitTime.TotalMilliseconds);
					}
				}
			}
			else if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Scheduled {0} tasks", numTasksRun);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="remainingMinimumWaitTime">The time that must elapse until the next task should be scheduled</param>
		/// <returns></returns>
		private int RunTasksIfPossible(out TimeSpan remainingMinimumWaitTime)
		{
			int tasksRun = 0;
			PeriodicTask task;
			while (_periodicTaskQueue.TryDequeuePendingTask(DateTime.Now, out task, out remainingMinimumWaitTime))
			{
				BeginRunTaskOnce(task);
				++tasksRun;
			}
			return tasksRun;
		}

		private void BeginRunTaskOnce(PeriodicTask task)
		{
			var actualTask = new Task(task.Run);
			actualTask.Start(_scheduler);
			actualTask.ContinueWith(unused => PeriodicTaskFinished(task));
		}

		private void PeriodicTaskFinished(PeriodicTask task)
		{
			_periodicTaskQueue.EnqueuePending(task);
			_taskAdded.Set();
		}
	}
}