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

		public bool TryMatchFormat(string fileName,
		                           byte[] header,
		                           Encoding encoding,
		                           out ILogFileFormat format,
		                           out Certainty certainty)
		{
			var formats = _repository.Formats.OfType<SerilogFileFormat>();

			foreach (var serilogFormat in formats)
			{
				var parser = serilogFormat.Parser;
				using (var memoryStream = new MemoryStream(header))
				using(var reader = new StreamReader(memoryStream, serilogFormat.Encoding ?? encoding))
				{
					if (TryParseFormat(reader, parser))
					{
						format = serilogFormat;
						certainty = Certainty.Sure;
						return true;
					}
				}
			}

			certainty = header.Length >= 512
				? Certainty.Sure
				: Certainty.Uncertain;
			format = null;
			return false;
		}

		private static bool TryParseFormat(StreamReader reader,
		                                   SerilogEntryParser parser)
		{
			// we're happy if we can match the first line
			var line = reader.ReadLine();
			if (!parser.TryParse(0, line, out _))
			{
				return false;
			}

			return true;
		}

		#endregion
	}
}