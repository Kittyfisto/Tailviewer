using System.IO;
using System.Text;
using Tailviewer.Plugins;

namespace Tailviewer.Test
{
	public sealed class SimpleLogFileFormatMatcher
		: ILogFileFormatMatcher
	{
		private readonly ILogFileFormat _format;

		public SimpleLogFileFormatMatcher(ILogFileFormat format)
		{
			_format = format;
		}

		#region Implementation of ILogFileFormatMatcher

		public bool TryMatchFormat(string fileName,
		                           Stream fileStream,
		                           Encoding encoding,
		                           out ILogFileFormat format)
		{
			format = _format;
			return format != null;
		}

		#endregion
	}
}