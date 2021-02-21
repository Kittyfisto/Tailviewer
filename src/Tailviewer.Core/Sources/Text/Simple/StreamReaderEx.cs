using System;
using System.IO;
using System.Text;

namespace Tailviewer.Core.Sources.Text.Simple
{
	/// <summary>
	///     Similar to <see cref="StreamReader" />, but is able to ReadLine() without
	///     removing newline characters from the string, allowing
	///     to differentiate between EoF with / and without newline.
	/// </summary>
	public sealed class StreamReaderEx
		: IDisposable
	{
		private readonly StringBuilder _contentBuffer;
		private readonly char[] _readBuffer;
		private readonly StreamReader _reader;
		private readonly Stream _stream;
		private int _nextSearchStartIndex;

		// We want to avoid allocating an array which is so big that it has to be placed in the large object heap because it would introduce a bit more memory fragmentation.
		// The total number of characters we may allocate here has been guesstimated using various sources on the internet.
		private const int MaxSmallObjectSize = 85000;
		private const int ObjectSize = 24;
		private const int EstimatedArraySize = 4;
		private const int CharSize = 2;
		private const int DefaultBufferSize = (MaxSmallObjectSize - ObjectSize - EstimatedArraySize - 1) / CharSize;

		/// <summary>
		///     Initializes this enhanced stream reader.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="encoding"></param>
		public StreamReaderEx(Stream stream, Encoding encoding)
			: this(stream, encoding, DefaultBufferSize)
		{}

		/// <summary>
		///     Initializes this enhanced stream reader.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="encoding"></param>
		/// <param name="bufferSize"></param>
		public StreamReaderEx(Stream stream, Encoding encoding, int bufferSize)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (encoding == null)
				throw new ArgumentNullException(nameof(encoding));

			_stream = stream;
			_reader = new StreamReader(_stream, encoding, detectEncodingFromByteOrderMarks: true);

			_readBuffer = new char[bufferSize];
			_contentBuffer = new StringBuilder();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_reader.Dispose();
			_stream.Dispose();
		}

		/// <summary>
		///     Reads the next line from the given stream.
		/// </summary>
		/// <returns></returns>
		public string ReadLine()
		{
			var line = TryFormLine();
			while (line == null && !_reader.EndOfStream)
			{
				var numRead = _reader.Read(_readBuffer, index: 0, count: _readBuffer.Length);
				_contentBuffer.Append(_readBuffer, startIndex: 0, charCount: numRead);

				line = TryFormLine();
			}

			return line;
		}

		private string TryFormLine()
		{
			var index = -1;
			for (var i = _nextSearchStartIndex; i < _contentBuffer.Length; ++i)
			{
				var c = _contentBuffer[i];
				if (c == '\n')
				{
					index = i;
					break;
				}
			}

			if (index == -1 && _reader.EndOfStream)
				index = _contentBuffer.Length - 1;

			if (index != -1)
				return FormLine(index + 1);

			_nextSearchStartIndex = _contentBuffer.Length;
			return null;
		}

		private string FormLine(int length)
		{
			var line = _contentBuffer.ToString(startIndex: 0, length: length);
			_contentBuffer.Remove(startIndex: 0, length: length);
			_nextSearchStartIndex = 0;
			return line;
		}
	}
}