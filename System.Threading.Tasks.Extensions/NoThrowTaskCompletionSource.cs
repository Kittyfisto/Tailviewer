namespace System.Threading.Tasks
{
	/// <summary>
	///     Similar to <see cref="TaskCompletionSource{T}" />, but produces <see cref="ITask" />s
	///     which require explicit checking of whether the task failed or not.
	/// </summary>
	public class NoThrowTaskCompletionSource
	{
		private readonly NoThrowTask _task;

		public NoThrowTaskCompletionSource()
		{
			_task = new NoThrowTask();
		}

		public ITask Task
		{
			get { return _task; }
		}

		public bool TrySetFinished()
		{
			return _task.TrySetFinished();
		}

		public bool TrySetCanceled()
		{
			return _task.TrySetCanceled();
		}

		public bool TrySetFaulted()
		{
			return _task.TrySetFaulted();
		}
	}

	/// <summary>
	///     Similar to <see cref="TaskCompletionSource{T}" />, but produces <see cref="ITask" />s
	///     which require explicit checking of whether the task failed or not.
	/// </summary>
	public sealed class NoThrowTaskCompletionSource<T>
	{
		private readonly NoThrowTask<T> _task;

		public NoThrowTaskCompletionSource()
		{
			_task = new NoThrowTask<T>();
		}

		public ITask<T> Task
		{
			get { return _task; }
		}

		public bool TrySetResult(T value)
		{
			return _task.TrySetFinished(value);
		}

		public bool TrySetCanceled()
		{
			return _task.TrySetCanceled();
		}

		public bool TrySetException(Exception e)
		{
			return _task.TrySetException(e);
		}
	}
}