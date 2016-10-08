using System;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.Scheduling
{
	/// <summary>
	///     Similar to <see cref="System.Threading.Tasks.TaskScheduler" />, it is capable of scheduling tasks.
	///     Can also schedule periodic tasks that are executed with a minimum time between them (until removed).
	///     <see cref="StartPeriodic" /> and <see cref="RemovePeriodic" />.
	/// </summary>
	public interface ITaskScheduler
	{
		/// <summary>
		///     Creates and starts a new task.
		///     The given <paramref name="callback" /> will be executed exactly once.
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		Task Start(Action callback);

		/// <summary>
		///     Creates and starts a new task.
		///     The given <paramref name="callback" /> will be executed exactly once.
		///     <see cref="Task{T}.Result" /> will carry the return value of the given callback, once its available.
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		Task<T> Start<T>(Func<T> callback);

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
		IPeriodicTask StartPeriodic(Action callback, TimeSpan minimumWaitTime, string name = null);

		/// <summary>
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		bool RemovePeriodic(IPeriodicTask task);
	}
}