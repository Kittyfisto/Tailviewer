using System.Text;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests
{
	public sealed class SimpleLogFileFormatMatcher
		: ILogFileFormatMatcher
	{
		public ILogFileFormat Format;
		public byte[] Header;
		public Encoding Encoding;
		public int NumInvocations;
		public Certainty Certainty;

		public SimpleLogFileFormatMatcher(ILogFileFormat format)
			: this(format, Certainty.Sure)
		{ }

		public SimpleLogFileFormatMatcher(ILogFileFormat format, Certainty certainty)
		{
			Format = format;
			Certainty = certainty;
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
			NumInvocations++;
			format = Format;
			certainty = Certainty;
			return format != null;
		}

		#endregion
	}
}