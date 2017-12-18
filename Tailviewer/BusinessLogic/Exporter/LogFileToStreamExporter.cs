using System;
using System.IO;
using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Exporter
{
	/// <summary>
	///     Exports the contents of a log file to a stream.
	/// </summary>
	public sealed class LogFileToStreamExporter
		: ILogFileExporter
	{
		private readonly ILogFile _logFile;
		private readonly Stream _stream;
		private bool _written;

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Does NOT take ownership of the given <paramref name="logFile" />, nor
		///     of the given <paramref name="stream" />.
		/// </remarks>
		/// <param name="logFile"></param>
		/// <param name="stream"></param>
		public LogFileToStreamExporter(ILogFile logFile, Stream stream)
		{
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (!stream.CanWrite)
				throw new ArgumentException("The given stream must be writable", nameof(stream));

			_logFile = logFile;
			_stream = stream;
		}

		public void Export(IProgress<Percentage> progressReporter = null)
		{
			const int bufferSize = 1000;
			var buffer = new LogLine[bufferSize];
			var count = _logFile.Count;
			var index = 0;

			using (var writer = new StreamWriter(_stream, Encoding.UTF8, 1024, true))
			{
				while (index < count)
				{
					var remaining = count - index;
					if (remaining > bufferSize)
						remaining = bufferSize;
					if (!ExportPortion(writer, buffer, index, remaining))
						break;

					index += remaining;
					var progress = Percentage.Of(index, count);
					progressReporter?.Report(progress);
				}
			}
		}

		private bool ExportPortion(StreamWriter writer, LogLine[] buffer, int index, int count)
		{
			try
			{
				_logFile.GetSection(new LogFileSection(index, count), buffer);

				for (var i = 0; i < count; ++i)
				{
					if (_written)
						writer.WriteLine();

					var logLine = buffer[i];
					writer.Write(logLine.Message);
					_written = true;
				}

				return true;
			}
			catch (IndexOutOfRangeException)
			{
				// The file has been invalidated and reduced in size already.
				// That's okay and we can end this export....
				return false;
			}
		}

	}
}