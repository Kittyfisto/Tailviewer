using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Tailviewer.Plugins;

namespace Tailviewer.Core.Sources.Text
{
	internal sealed class FileFormatDetector
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileFormatMatcher _formatMatcher;
		private readonly EncodingDetector _encodingDetector;
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
			// TODO: move up
			_encodingDetector = new EncodingDetector();
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
				_encoding = PickEncoding(_format, _encodingDetector.TryFindEncoding(stream));
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

			if (detectedEncoding != null)
				return detectedEncoding;

			Log.DebugFormat("File {0}: No encoding could be determined, falling back to {1}", _fileName, _defaultEncoding);
			return _defaultEncoding;
		}
	}
}