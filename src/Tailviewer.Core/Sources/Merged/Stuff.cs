using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.Core.Sources.Merged
{
	/// <summary>
	///     Keeps track of which indices of a source log file are being requested,
	///     as well as their index into a destination buffer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class Stuff<T>
	{
		private readonly List<LogLineIndex> _originalLogLineIndices;
		private readonly List<int> _destinationIndices;

		public IReadOnlyList<int> DestinationIndices => _destinationIndices;

		private T[] _buffer;

		public Stuff()
		{
			_destinationIndices = new List<int>();
			_originalLogLineIndices = new List<LogLineIndex>();
		}

		public IReadOnlyList<LogLineIndex> OriginalLogLineIndices => _originalLogLineIndices;

		public T[] Buffer
		{
			get
			{
				if (_buffer == null)
				{
					_buffer = new T[_originalLogLineIndices.Count];
				}
				return _buffer;
			}
		}

		public void Add(int destIndex, LogLineIndex index)
		{
			_destinationIndices.Add(destIndex);
			_originalLogLineIndices.Add(index);
		}
	}
}