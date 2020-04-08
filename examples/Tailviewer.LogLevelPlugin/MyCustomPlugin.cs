using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.LogLevelPlugin
{
	public sealed class MyCustomPlugin
		: IFileFormatPlugin2
	{
		#region Implementation of IFileFormatPlugin

		public IReadOnlyList<string> SupportedExtensions
		{
			get
			{
				// This causes Open() to be called for every file with an extension of mylog.
				// For example foo.mylog, logfile.mylog as well as .mylog
				return new []{"mylog"};
				// If you want to use regular expressions only, you may return an empty list here.
				// return new string[0];
			}
		}

		public IReadOnlyList<Regex> SupportedFileNames
		{
			get
			{
				// This causes Open() to be called for every file which matches the following regex
				return new[] { new Regex("log_*\\.logv2") };
				// If you want to use a list of file extensions only, you may return an empty list here.
				// return new string[0];
			}
		}

		public ILogFile Open(IServiceContainer services, string fileName)
		{
			// Registering an instance which implements ILogLineTranslator causes
			// the call to CreateTextLogFile to use this translator to translate every
			// reported log line, allowing a plugin to:
			// - Change the content being displayed
			// - Parse metadata, thereby allowing tailviewer to properly interpret the log file
			//   - Parsing timestamps allows treating multiple lines as a single log entry
			//   - Parsing timestamps allows multiple log files to be merged into chronological order
			//   - Parsing log levels allows highlighting log entries and easy filtering by log level
			services.RegisterInstance<ILogLineTranslator>(new MyCustomLogLevelTranslator());

			return services.CreateTextLogFile(fileName);
		}

		#endregion
	}
}