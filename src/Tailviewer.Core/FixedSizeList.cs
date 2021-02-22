namespace Tailviewer.Core
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class FixedSizeList<T>
	{
		private readonly T[] _buffer;

		private int _count;

		/// <summary>
		/// </summary>
		public FixedSizeList(int length)
		{
			_buffer = new T[length];
		}

		public T[] Buffer
		{
			get { return _buffer; }
		}

		/// <summary>
		///     The number of values in this list.
		/// </summary>
		public int Count
		{
			get { return _count; }
		}

		/// <summary>
		/// </summary>
		/// <param name="value"></param>
		public bool TryAdd(T value)
		{
			if (_count >= _buffer.Length)
				return false;

			_buffer[_count] = value;
			++_count;
			return true;
		}

		/// <summary>
		/// </summary>
		public void Clear()
		{
			_count = 0;
		}
	}
}