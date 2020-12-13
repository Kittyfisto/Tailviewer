using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

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

		public bool TryMatchFormat(string fileName, byte[] initialContent, out ILogFileFormat format)
		{
			format = _format;
			return format != null;
		}

		#endregion
	}
}