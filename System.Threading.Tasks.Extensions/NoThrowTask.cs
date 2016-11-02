namespace System.Threading.Tasks
{
	/// <summary>
	///     Similar to <see cref="NoThrowTask" />, but doesn't throw exceptions when the task was cancelled or faulted.
	///     Should obvioulsy only used in cases where a caller can't handle different exceptions in meaningful ways
	///     (besides logging, which the caller already does..).
	/// </summary>
	internal class NoThrowTask
		: ITask
	{
		private readonly ManualResetEventSlim _finishedEvent;
		private readonly object _syncRoot;
		private bool _isCanceled;
		private bool _isCompleted;
		private bool _isFaulted;

		public NoThrowTask()
		{
			_finishedEvent = new ManualResetEventSlim();
			_syncRoot = new object();
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
			_finishedEvent.Wait();
		}

		public bool Wait(TimeSpan timeout)
		{
			return _finishedEvent.Wait(timeout);
		}

		public bool TrySetFinished()
		{
			lock (_syncRoot)
			{
				if (_isCanceled || _isCompleted || _isFaulted)
					return false;

				_isCompleted = true;
			}

			_finishedEvent.Set();
			return true;
		}

		public bool TrySetCanceled()
		{
			lock (_syncRoot)
			{
				if (_isCanceled || _isCompleted || _isFaulted)
					return false;

				_isCanceled = true;
			}

			_finishedEvent.Set();
			return true;
		}

		public bool TrySetFaulted()
		{
			lock (_syncRoot)
			{
				if (_isCanceled || _isCompleted || _isFaulted)
					return false;

				_isFaulted = true;
			}

			_finishedEvent.Set();
			return true;
		}
	}

	internal class NoThrowTask<T>
		: ITask<T>
	{
		private readonly ManualResetEventSlim _finishedEvent;
		private readonly object _syncRoot;
		private bool _isCanceled;
		private bool _isCompleted;
		private bool _isFaulted;
		private T _result;

		public NoThrowTask()
		{
			_finishedEvent = new ManualResetEventSlim();
			_syncRoot = new object();
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
			_finishedEvent.Wait();
		}

		public bool Wait(TimeSpan timeout)
		{
			return _finishedEvent.Wait(timeout);
		}

		public T Result
		{
			get
			{
				Wait();
				return _result;
			}
		}

		public bool TrySetFinished(T result)
		{
			lock (_syncRoot)
			{
				if (_isCanceled || _isCompleted || _isFaulted)
					return false;

				_isCompleted = true;
				_result = result;
			}

			_finishedEvent.Set();
			return true;
		}

		public bool TrySetCanceled()
		{
			lock (_syncRoot)
			{
				if (_isCanceled || _isCompleted || _isFaulted)
					return false;

				_isCanceled = true;
			}

			_finishedEvent.Set();
			return true;
		}

		public bool TrySetException(Exception exception)
		{
			lock (_syncRoot)
			{
				if (_isCanceled || _isCompleted || _isFaulted)
					return false;

				_isFaulted = true;
			}

			_finishedEvent.Set();
			return true;
		}
	}
}