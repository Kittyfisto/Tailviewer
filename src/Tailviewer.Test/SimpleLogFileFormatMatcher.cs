using System.Text;
using Tailviewer.Plugins;

namespace Tailviewer.Test
{
	public sealed class SimpleLogFileFormatMatcher
		: ILogFileFormatMatcher
	{
		private readonly ILogFileFormat _format;
		public byte[] Header;
		public Encoding Encoding;

		public SimpleLogFileFormatMatcher(ILogFileFormat format)
		{
			_format = format;
		}

		#region Implementation of ILogFileFormatMatcher

		public bool TryMatchFormat(string fileName,
		                           byte[] header,
		                           Encoding encoding,
		                           out ILogFileFormat format,
		                           out Certainty certainty)
		{
			Header = header;
			Encoding = encoding;

			format = _format;
			certainty = Certainty.Sure;
			return format != null;
		}

		#endregion
	}
}