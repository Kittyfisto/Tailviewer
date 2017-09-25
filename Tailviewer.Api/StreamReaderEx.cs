using System;
using System.IO;
using System.Text;

namespace Tailviewer
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

		/// <summary>
		///     Initializes this enhanced stream reader.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="encoding"></param>
		public StreamReaderEx(Stream stream, Encoding encoding)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (encoding == null)
				throw new ArgumentNullException(nameof(encoding));

			_stream = stream;
			_reader = new StreamReader(_stream, encoding, true);

			_readBuffer = new char[1024];
			_contentBuffer = new StringBuilder();
		}

		public void Dispose()
		{
			_reader.Dispose();
			_stream.Dispose();
		}

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
			for (var i = 0; i < _contentBuffer.Length; ++i)
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

			return null;
		}

		private string FormLine(int length)
		{
			var line = _contentBuffer.ToString(startIndex: 0, length: length);
			_contentBuffer.Remove(startIndex: 0, length: length);
			return line;
		}
	}
}