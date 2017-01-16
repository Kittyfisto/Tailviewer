namespace System.Threading.Tasks
{
	public static class Task2
	{
		public static ITask<T> FromResult<T>(T result)
		{
			return new Task2<T>(result);
		}

		public static ITask<T> FromFailure<T>()
		{
			return new Task2<T>(default(T), isFaulted: true);
		}
	}

	public sealed class Task2<T>
		: ITask<T>
	{
		private readonly T _result;
		private readonly bool _isCompleted;
		private readonly bool _isFaulted;
		private bool _isCanceled;

		internal Task2(T result, bool isFaulted = false)
		{
			_result = result;
			_isCompleted = true;
			_isFaulted = isFaulted;
		}

		public bool IsCompleted
		{
			get { return _isCompleted; }
		}

		public bool IsCanceled
		{
			get { return _isCanceled; }
		}

		public bool IsFaulted
		{
			get { return _isFaulted; }
		}

		public void Wait()
		{
			
		}

		public bool Wait(TimeSpan timeout)
		{
			return true;
		}

		public T Result
		{
			get { return _result; }
		}
	}
}