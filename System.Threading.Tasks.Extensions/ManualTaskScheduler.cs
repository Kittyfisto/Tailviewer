using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks
{
	/// <summary>
	///     <see cref="ITaskScheduler" /> implementation that allows the user to control when and if tasks are executed.
	/// </summary>
	public sealed class ManualTaskScheduler
		: ITaskScheduler
	{
		private readonly object _syncRoot;
		private readonly List<PeriodicTask> _tasks;
		private long _lastId;

		public ManualTaskScheduler()
		{
			_syncRoot = new object();
			_tasks = new List<PeriodicTask>();
		}

		public int PeriodicTaskCount
		{
			get
			{
				lock (_syncRoot)
				{
					return _tasks.Count;
				}
			}
		}

		public IEnumerable<IPeriodicTask> PeriodicTasks
		{
			get
			{
				lock (_syncRoot)
				{
					return _tasks.ToList();
				}
			}
		}

		public Task Start(Action callback)
		{
			return Task.Factory.StartNew(callback);
		}

		public Task<T> Start<T>(Func<T> callback)
		{
			return Task.Factory.StartNew(callback);
		}

		public IPeriodicTask StartPeriodic(Action callback, TimeSpan minimumWaitTime, string name = null)
		{
			var task = new PeriodicTask(Interlocked.Increment(ref _lastId), callback, minimumWaitTime, name);
			lock (_syncRoot)
			{
				_tasks.Add(task);
			}
			return task;
		}

		public IPeriodicTask StartPeriodic(Func<TimeSpan> callback, string name = null)
		{
			var task = new PeriodicTask(Interlocked.Increment(ref _lastId), callback, name);
			lock (_syncRoot)
			{
				_tasks.Add(task);
			}
			return task;
		}

		public bool StopPeriodic(IPeriodicTask task)
		{
			var actualTask = task as PeriodicTask;
			if (actualTask == null)
				return false;

			lock (_syncRoot)
			{
				return _tasks.Remove(actualTask);
			}
		}

		/// <summary>
		///     Runs every task exactly once.
		/// </summary>
		public void RunOnce()
		{
			IEnumerable<PeriodicTask> tasks;
			lock (_syncRoot)
			{
				tasks = _tasks.ToList();
			}

			foreach (PeriodicTask task in tasks)
			{
				task.Run();
			}
		}

		public void Run(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				RunOnce();
			}
		}
	}
}