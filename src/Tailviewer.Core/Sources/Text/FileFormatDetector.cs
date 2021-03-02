using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Tailviewer.Api;

namespace Tailviewer.Core.Sources.Text
{
	internal sealed class FileFormatDetector
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileFormatMatcher _formatMatcher;
		private Certainty _detectionCertainty;
		private ILogFileFormat _format;

		public FileFormatDetector(ILogFileFormatMatcher formatMatcher)
		{
			_formatMatcher = formatMatcher;
		}

		/// <summary>
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="fileStream"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public ILogFileFormat TryDetermineFormat(string fileName,
		                                         Stream fileStream,
		                                         Encoding encoding)
		{
			try
			{
				if (_format == null || _detectionCertainty != Certainty.Sure)
					_format = TryFindFormatOf(fileName, fileStream, encoding, out _detectionCertainty);
			}
			catch (IOException e)
			{
				Log.DebugFormat("Caught exception: {0}", e);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}

			return _format;
		}

		[Pure]
		private ILogFileFormat TryFindFormatOf(string fileName,
		                                       Stream stream,
		                                       Encoding encoding,
		                                       out Certainty certainty)
		{
			stream.Position = 0;

			const int maxHeaderLength = 512;
			var length = Math.Min(maxHeaderLength, stream.Length);
			var header = new byte[length];
			stream.Read(header, offset: 0, header.Length);

			_formatMatcher.TryMatchFormat(fileName, header, encoding, out var format, out certainty);
			if (format != null)
				return format;

			certainty = header.Length >= maxHeaderLength
				? Certainty.Sure
				: Certainty.Uncertain;
			return LogFileFormats.GenericText;
		}
	}
}