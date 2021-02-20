﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Plugins;

namespace Tailviewer.BusinessLogic.Sources
{
	/// <summary>
	///     Responsible for matching a log file to a particular format, if possible.
	/// </summary>
	internal sealed class LogFileFormatMatcher
		: ILogFileFormatMatcher
	{
		private readonly IReadOnlyList<ILogFileFormatMatcher> _matchers;

		public LogFileFormatMatcher(IServiceContainer services)
		{
			var pluginLoader = services.Retrieve<IPluginLoader>();
			_matchers = pluginLoader.LoadAllOfType<ILogFileFormatMatcherPlugin>()
			                        .Select(x => new NoThrowLogFileFormatMatcher(x, services))
			                        .ToList();
		}

		#region Implementation of ILogFileFormatMatcher

		public bool TryMatchFormat(string fileName,
		                           Stream fileStream,
		                           Encoding encoding,
		                           out ILogFileFormat format)
		{
			foreach (var matcher in _matchers)
				if (matcher.TryMatchFormat(fileName, fileStream, encoding, out format))
					return true;

			format = null;
			return false;
		}

		#endregion
	}
}