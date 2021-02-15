using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.LogFiles.Text
{
	internal sealed class FileFormatDetector
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileFormatMatcher _formatMatcher;
		private readonly string _fileName;
		private readonly Encoding _defaultEncoding;
		private ILogFileFormat _format;
		private Certainty _detectionCertainty;
		private Encoding _encoding;
		private FileFingerprint _lastFingerprint;

		public FileFormatDetector(ILogFileFormatMatcher formatMatcher,
		                          string fileName,
		                          Encoding defaultEncoding)
		{
			_formatMatcher = formatMatcher;
			_fileName = fileName;
			_defaultEncoding = defaultEncoding;
			_encoding = defaultEncoding;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public ILogFileFormat TryDetermineFormat(out Encoding encoding)
		{
			try
			{
				if (!File.Exists(_fileName))
				{
					_lastFingerprint = null;
				}
				else
				{
					var currentFingerprint = FileFingerprint.FromFile(_fileName);
					if (_format == null || _detectionCertainty != Certainty.Sure ||
					    !Equals(currentFingerprint, _lastFingerprint))
					{
						DetermineFormat();
						_lastFingerprint = currentFingerprint;
					}
				}
			}
			catch (IOException e)
			{
				Log.DebugFormat("Caught exception: {0}", e);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}

			encoding = _encoding;
			return _format;
		}

		private void DetermineFormat()
		{
			using (var stream = new FileStream(_fileName,
			                                   FileMode.Open,
			                                   FileAccess.Read,
			                                   FileShare.ReadWrite))
			{
				DetermineFormat(stream);
			}
		}

		private void DetermineFormat(FileStream stream)
		{
			if (_format == null || _detectionCertainty != Certainty.Sure)
			{
				_format = TryFindFormatOf(stream, out _detectionCertainty);
				_encoding = PickEncoding(_format, TryFindEncodingOf(stream));
			}
		}

		[Pure]
		private ILogFileFormat TryFindFormatOf(FileStream stream, out Certainty certainty)
		{
			var pos = stream.Position;

			const int maxHeaderLength = 512;
			var length = Math.Min(maxHeaderLength, stream.Length - pos);
			var header = new byte[length];
			stream.Read(header, 0, header.Length);
			certainty = length >= maxHeaderLength
				? Certainty.Sure
				: Certainty.Uncertain;

			_formatMatcher.TryMatchFormat(_fileName, header, out var format);
			if (format != null)
				return format;

			return LogFileFormats.GenericText;
		}

		private Encoding TryFindEncodingOf(FileStream stream)
		{
			stream.Position = 0;
			byte[] buffer = new byte[5];
			stream.Read(buffer, 0, 5);

			if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
				return Encoding.UTF8;

			if (buffer[0] == 0xfe && buffer[1] == 0xff)
				return Encoding.Unicode;

			if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
				return Encoding.UTF32;

			if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
				return Encoding.UTF7;

			return null;
		}

		[Pure]
		private Encoding PickEncoding(ILogFileFormat format, Encoding detectedEncoding)
		{
			var formatEncoding = format.Encoding;
			if (formatEncoding != null)
			{
				if (detectedEncoding != null)
				{
					Log.WarnFormat("File {0} has been detected to be encoded with {1}, but its format ({2}) says it's encoded with {3}, choosing the latter....",
					               _fileName,
					               detectedEncoding.WebName,
					               format,
					               formatEncoding.WebName);
				}

				return formatEncoding;
			}

			return detectedEncoding;
		}
	}
}