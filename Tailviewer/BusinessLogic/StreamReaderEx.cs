using System;
using System.IO;
using System.Text;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Similar to <see cref="StreamReader" />, but is able to ReadLine() without
	///     removing newline characters from the string, allowing
	///     to differentiate between EoF with / and without newline.
	/// </summary>
	internal sealed class StreamReaderEx
		: IDisposable
	{
		private readonly StringBuilder _contentBuffer;
		private readonly char[] _readBuffer;
		private readonly StreamReader _reader;
		private readonly Stream _stream;

		public StreamReaderEx(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			_stream = stream;
			_reader = new StreamReader(_stream);

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
			string line = TryFormLine();
			while (line == null && !_reader.EndOfStream)
			{
				var numRead = _reader.Read(_readBuffer, 0, _readBuffer.Length);
				_contentBuffer.Append(_readBuffer, 0, numRead);

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
				if (c < 32)
				{
					int n = 0;
				}

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
			var line = _contentBuffer.ToString(0, length);
			_contentBuffer.Remove(0, length);
			return line;
		}
	}
}