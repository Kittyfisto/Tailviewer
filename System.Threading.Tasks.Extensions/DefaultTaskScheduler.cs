using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace System.Threading.Tasks
{
	/// <summary>
	///     Similar to <see cref="System.Threading.Tasks.TaskScheduler" />, it is capable of scheduling tasks.
	///     Can also schedule periodic tasks that are executed with a minimum time between them (until removed).
	///     <see cref="StartPeriodic" /> and <see cref="StopPeriodic" />.
	/// </summary>
	public sealed class DefaultTaskScheduler
		: ITaskScheduler
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly TaskScheduler _scheduler;
		private readonly List<PeriodicTask> _tasks;
		private long _lastTaskId;

		public DefaultTaskScheduler()
			: this(TaskScheduler.Default)
		{}

		private DefaultTaskScheduler(TaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");

			_scheduler = scheduler;
			_tasks = new List<PeriodicTask>();
		}

		public void Dispose()
		{
			lock (_tasks)
			{
				foreach (var task in _tasks)
				{
					task.IsRemoved = true;
				}
			}
		}

		public int PeriodicTaskCount
		{
			get
			{
				lock (_tasks)
				{
					return _tasks.Count;
				}
			}
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
		///     <see cref="NoThrowTask{T}.Result" /> will carry the return value of the given callback, once its available.
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

			lock (_tasks)
			{
				_tasks.Add(task);
			}

			RunOnce(task);

			return task;
		}

		public IPeriodicTask StartPeriodic(Func<TimeSpan> callback, string name = null)
		{
			long id = Interlocked.Increment(ref _lastTaskId);
			var task = new PeriodicTask(id, callback, name);

			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Starting periodic task {0} at irregular intervals", task);
			}

			lock (_tasks)
			{
				_tasks.Add(task);
			}

			RunOnce(task);

			return task;
		}

		/// <summary>
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public bool StopPeriodic(IPeriodicTask task)
		{
			var periodicTask = task as PeriodicTask;
			if (periodicTask == null)
				return false;

			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Removing periodic task {0}", task);
			}

			lock (_tasks)
			{
				return _tasks.Remove(periodicTask);
			}
		}

		private void RunOnce(PeriodicTask periodicTask)
		{
			var remainingWaitTime = periodicTask.RemainingTimeUntilNextInvocation;
			var waitTask = Task.Delay(remainingWaitTime);
			var actualTask = waitTask.ContinueWith(unused =>
				{
					periodicTask.Run();
					return periodicTask;
				}, _scheduler);
			actualTask.ContinueWith(OnPeriodicTaskFinished);
		}

		private void OnPeriodicTaskFinished(Task<PeriodicTask> task)
		{
			var periodicTask = task.Result;

			if (periodicTask.IsRemoved)
			{
				Log.DebugFormat("Periodic task '{0}' has been removed and will no longer be scheduled", task);
			}
			else
			{
				if (Log.IsDebugEnabled)
				{
					Log.DebugFormat("Periodic task '{0}' has finished executing and is added to the task queue once more", task);
				}

				RunOnce(periodicTask);
			}
		}
	}
}