namespace System.Threading.Tasks
{
	/// <summary>
	///     Similar to <see cref="System.Threading.Tasks.TaskScheduler" />, it is capable of scheduling tasks.
	///     Can also schedule periodic tasks that are executed with a minimum time between them (until removed).
	///     <see cref="StartPeriodic" /> and <see cref="StopPeriodic" />.
	/// </summary>
	public interface ITaskScheduler
	{
		/// <summary>
		///     The amount of periodic tasks currently running.
		/// </summary>
		int PeriodicTaskCount { get; }

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
		///     <see cref="NoThrowTask{T}.Result" /> will carry the return value of the given callback, once its available.
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		Task<T> Start<T>(Func<T> callback);

		/// <summary>
		///     Creates and starts a new periodic task.
		///     Periodic tasks (as the name suggests) are tasks that are periodically invoked (instead of just once).
		/// </summary>
		/// <remarks>
		///     Contrary to timers, the <paramref name="callback" /> is never invoked in parallel.
		///     If the callback's execution time is greater than the minimum wait time (or the system is saturated) then
		///     the callback will be invoked as fast as possible, but no additional invocations queue up.
		/// </remarks>
		/// <param name="callback"></param>
		/// <param name="minimumWaitTime">
		///     The minimum time that should ellapsed between two successive calls to <paramref name="callback" />
		/// </param>
		/// <param name="name">The name of the task, for debugging purposes</param>
		/// <returns></returns>
		IPeriodicTask StartPeriodic(Action callback, TimeSpan minimumWaitTime, string name = null);

		/// <summary>
		///     Creates and starts a new periodic task.
		///     Periodic tasks (as the name suggests) are tasks that are periodically invoked (instead of just once).
		/// </summary>
		/// <remarks>
		///     Contrary to timers, the <paramref name="callback" /> is never invoked in parallel.
		///     If the callback's execution time is greater than the minimum wait time (or the system is saturated) then
		///     the callback will be invoked as fast as possible, but no additional invocations queue up.
		/// </remarks>
		/// <remarks>
		///     The callback gets to decide when it wants to be scheduled the next time.
		/// </remarks>
		/// <param name="callback"></param>
		/// <param name="name">The name of the task, for debugging purposes</param>
		/// <returns></returns>
		IPeriodicTask StartPeriodic(Func<TimeSpan> callback, string name = null);

		/// <summary>
		///     Removes a periodic task.
		///     Its callback will no longer be invoked (eventually).
		/// </summary>
		/// <remarks>
		///     DOES NOT BLOCK UNTIL THE CALLBACK IS FINISHED.
		/// </remarks>
		/// <param name="task"></param>
		/// <returns></returns>
		bool StopPeriodic(IPeriodicTask task);
	}
}