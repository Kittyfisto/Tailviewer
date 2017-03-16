namespace System.Threading.Tasks
{
	/// <summary>
	/// 
	/// </summary>
	public interface IOneShotTask
	{
		/// <summary>
		///     The name of this task for debugging purposes.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The amount of time that should still ellapse until the invocation of this one shot task.
		/// </summary>
		TimeSpan RemainingTimeUntilInvocation { get; }
	}
}