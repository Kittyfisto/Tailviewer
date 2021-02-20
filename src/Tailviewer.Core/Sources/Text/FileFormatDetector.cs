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
			var pos = stream.Position;

			const int maxHeaderLength = 512;
			var length = Math.Min(maxHeaderLength, stream.Length - pos);
			var header = new byte[length];
			stream.Read(header, offset: 0, header.Length);
			certainty = length >= maxHeaderLength
				? Certainty.Sure
				: Certainty.Uncertain;

			_formatMatcher.TryMatchFormat(fileName, stream, encoding, out var format);
			if (format != null)
				return format;

			return LogFileFormats.GenericText;
		}
	}
}