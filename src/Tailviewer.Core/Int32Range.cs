using System.Collections;
using System.Collections.Generic;

namespace Tailviewer.Core
{
	/// <summary>
	///     
	/// </summary>
	public sealed class Int32Range
		: IReadOnlyList<int>
	{
		private readonly int _offset;
		private readonly int _count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public Int32Range(int offset, int count)
		{
			_offset = offset;
			_count = count;
		}

		/// <inheritdoc />
		public IEnumerator<int> GetEnumerator()
		{
			return new Enumerator(_offset, _count);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public int Count => _count;

		/// <inheritdoc />
		public int this[int index] => (int) _offset + index;

		private sealed class Enumerator
			: IEnumerator<int>
		{
			private readonly int _offset;
			private readonly int _count;
			private int _index;

			public Enumerator(int offset, int count)
			{
				_offset = offset;
				_count = count;
				_index = offset - 1;
			}

			public void Dispose()
			{}

			public bool MoveNext()
			{
				if (++_index >= _offset + _count)
					return false;

				return true;
			}

			public void Reset()
			{
				_index = _offset - 1;
			}

			public int Current => _offset + _index;

			object IEnumerator.Current => Current;
		}
	}
}