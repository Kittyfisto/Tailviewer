using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class LineOffsetDetector
	{
		private readonly Stream _stream;
		private readonly byte[] _pattern;
		private readonly byte[] _buffer;
		private long _bufferOffset;
		private int _bufferPos;
		private int _bufferLength;
		private int _numMatchedInSequence;

		public LineOffsetDetector(Stream stream, Encoding encoding, int bufferSize = 4096)
		{
			_stream = stream;
			_pattern = encoding.GetBytes("\n");
			//var pattern = encoding.GetBytes("\n");
			//if (pattern.Length > 1)
			//	throw new NotImplementedException($"Line offset detection not implemented for {encoding.WebName}");
			//_pattern = pattern[0];

			_buffer = new byte[bufferSize];
			FillBufferIfNecessary();
		}

		/// <summary>
		///    Returns the number of bytes to the start of the stream where the next line occurs.
		/// </summary>
		/// <returns></returns>
		public long FindNextLineOffset()
		{
			do
			{
				FillBufferIfNecessary();

				for (; _bufferPos < _bufferLength; ++_bufferPos)
				{
					if (_buffer[_bufferPos] == _pattern[_numMatchedInSequence])
					{
						++_numMatchedInSequence;

						if (_numMatchedInSequence == _pattern.Length)
						{
							++_bufferPos;
							_numMatchedInSequence = 0;
							return _bufferOffset + _bufferPos;
						}
					}
					else
					{
						_numMatchedInSequence = 0;
					}
				}
			} while (_bufferLength > 0);

			return -1;
		}

		private void FillBufferIfNecessary()
		{
			if (_bufferPos < _bufferLength)
				return;

			_bufferOffset = _stream.Position;
			_bufferLength = _stream.Read(_buffer, 0, _buffer.Length);
			_bufferPos = 0;
		}
	}
}
