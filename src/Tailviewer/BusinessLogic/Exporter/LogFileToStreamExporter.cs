using System;
using System.IO;
using System.Text;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Exporter
{
	/// <summary>
	///     Exports the contents of a log file to a stream.
	/// </summary>
	public sealed class LogFileToStreamExporter
		: ILogFileExporter
	{
		private readonly ILogSource _logSource;
		private readonly Stream _stream;
		private bool _written;

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Does NOT take ownership of the given <paramref name="logSource" />, nor
		///     of the given <paramref name="stream" />.
		/// </remarks>
		/// <param name="logSource"></param>
		/// <param name="stream"></param>
		public LogFileToStreamExporter(ILogSource logSource, Stream stream)
		{
			if (logSource == null)
				throw new ArgumentNullException(nameof(logSource));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (!stream.CanWrite)
				throw new ArgumentException("The given stream must be writable", nameof(stream));

			_logSource = logSource;
			_stream = stream;
		}

		public void Export(IProgress<Percentage> progressReporter = null)
		{
			const int bufferSize = 1000;
			var buffer = new LogBufferArray(bufferSize, GeneralColumns.Index, GeneralColumns.RawContent);
			var count = _logSource.GetProperty(GeneralProperties.LogEntryCount);
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

		private bool ExportPortion(StreamWriter writer, LogBufferArray array, int index, int count)
		{
			try
			{
				_logSource.GetEntries(new LogSourceSection(index, count), array);

				for (var i = 0; i < count; ++i)
				{
					if (_written)
						writer.WriteLine();

					var logLine = array[i];
					if (logLine.Index == LogLineIndex.Invalid) //< EOF
						break;

					writer.Write(logLine.RawContent);
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