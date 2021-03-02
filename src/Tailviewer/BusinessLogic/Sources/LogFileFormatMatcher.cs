using System.Linq;
using System.Text;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.BusinessLogic.Sources
{
	/// <summary>
	///     Responsible for matching a log file to a particular format, if possible.
	/// </summary>
	internal sealed class LogFileFormatMatcher
		: ILogFileFormatMatcher
	{
		private readonly IServiceContainer _services;

		public LogFileFormatMatcher(IServiceContainer services)
		{
			_services = services;
		}

		#region Implementation of ILogFileFormatMatcher

		public bool TryMatchFormat(string fileName,
		                           byte[] header,
		                           Encoding encoding,
		                           out ILogFileFormat format,
		                           out Certainty certainty)
		{
			var pluginLoader = _services.Retrieve<IPluginLoader>();
			var matchers = pluginLoader.LoadAllOfType<ILogFileFormatMatcherPlugin>()
			                        .Select(x => new NoThrowLogFileFormatMatcher(x, _services))
			                        .ToList();

			foreach (var matcher in matchers)
				if (matcher.TryMatchFormat(fileName, header, encoding, out format, out certainty))
					return true;

			format = null;
			certainty = Certainty.None;
			return false;
		}

		#endregion
	}
}