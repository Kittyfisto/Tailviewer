using System.IO;
using System.Linq;
using System.Text;
using Tailviewer.Plugins;

namespace Tailviewer.Formats.Serilog
{
	/// <summary>
	///     Responsible for finding out if a particular log file matches any of the well known serilog formats.
	/// </summary>
	public sealed class SerilogFileFormatMatcher
		: ILogFileFormatMatcher
	{
		private readonly ILogFileFormatRepository _repository;

		public SerilogFileFormatMatcher(ILogFileFormatRepository repository)
		{
			_repository = repository;
		}

		#region Implementation of ILogFileFormatMatcher

		public bool TryMatchFormat(string fileName, byte[] initialContent, out ILogFileFormat format)
		{
			var formats = _repository.Formats.OfType<SerilogFileFormat>();

			foreach (var serilogFormat in formats)
			{
				var parser = serilogFormat.Parser;
				using (var reader = CreateStreamReader(initialContent, serilogFormat.Encoding))
				{
					if (TryParseFormat(reader, parser))
					{
						format = serilogFormat;
						return true;
					}
				}
			}

			format = null;
			return false;
		}

		private static bool TryParseFormat(StreamReader reader,
		                                   SerilogFileParser parser)
		{
			// we're happy if we can match the first line
			var line = reader.ReadLine();
			if (!parser.TryParse(line, out _))
			{
				return false;
			}

			return true;
		}

		private static StreamReader CreateStreamReader(byte[] initialContent, Encoding encoding)
		{
			var stream = new MemoryStream(initialContent);
			if (encoding != null)
				return new StreamReader(stream, encoding);

			return new StreamReader(stream);
		}

		#endregion
	}
}