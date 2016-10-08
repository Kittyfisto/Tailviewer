using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	internal sealed class SingleDataSource
		: AbstractDataSource
	{
		private readonly ILogFile _logFile;

		public SingleDataSource(DataSource settings)
			: this(settings, TimeSpan.FromMilliseconds(100))
		{
		}

		public SingleDataSource(DataSource settings, TimeSpan maximumWaitTime)
			: base(settings, maximumWaitTime)
		{
			var logFile = new LogFile(settings.File);
			logFile.Start();
			_logFile = logFile;
			CreateFilteredLogFile();
		}

		public SingleDataSource(DataSource settings, ILogFile logFile, TimeSpan maximumWaitTime)
			: base(settings, maximumWaitTime)
		{
			_logFile = logFile;
			CreateFilteredLogFile();
		}

		public override ILogFile LogFile
		{
			get { return _logFile; }
		}
	}
}