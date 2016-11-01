namespace System.Threading.Tasks
{
	public interface IPeriodicTask
	{
		/// <summary>
		///     The name of this task for debugging purposes.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The number of failures that occured while updating this task.
		///     More specifically, the number of exceptions that were thrown by the task's update function.
		/// </summary>
		int NumFailures { get; }

		/// <summary>
		///     The amount of time that should still ellapse until the next invocation of this periodic task.
		/// </summary>
		TimeSpan RemainingTimeUntilNextInvocation { get; }
	}
}