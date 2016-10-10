namespace System.Threading.Tasks
{
	public interface IPeriodicTask
	{
		/// <summary>
		///     The name of this task for debugging purposes.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The amount of time that should still ellapse until the next invocation of this periodic task.
		/// </summary>
		TimeSpan RemainingTimeUntilNextInvocation { get; }
	}
}