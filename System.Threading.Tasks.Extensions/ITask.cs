namespace System.Threading.Tasks
{
	/// <summary>
	///     The interface for an asynchronous operation where the caller is NOT interested
	///     in why the operation failed. Contrary to <see cref="NoThrowTask" />, failures are NOT
	///     communicated by throwing exceptions from <see cref="ITask.Wait()" />, but
	///     have to be queried EXCPLITLY.
	/// </summary>
	public interface ITask
	{
		bool IsCompleted { get; }
		bool IsCanceled { get; }
		bool IsFaulted { get; }

		void Wait();
		bool Wait(TimeSpan timeout);
	}

	/// <summary>
	///     The interface for a future where the caller is NOT interested
	///     in why the operation failed. Contrary to <see cref="NoThrowTask{T}" />, failures are NOT
	///     communicated by throwing exceptions from <see cref="ITask.Wait()" />, but
	///     have to be queried EXCPLITLY.
	/// </summary>
	public interface ITask<out T>
		: ITask
	{
		/// <summary>
		///     The result of the task or default(T) when the operation failed or was cancelled.
		///     <see cref="ITask.IsFaulted" /> and <see cref="ITask.IsCanceled" /> should be queried
		///     to make sure that the result may be used.
		/// </summary>
		T Result { get; }
	}
}