using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	internal sealed class SingleDataSource
		: AbstractDataSource
	{
		private readonly LogFile _logFile;

		public SingleDataSource(DataSource settings)
			: this(settings, TimeSpan.FromMilliseconds(100))
		{
		}

		public SingleDataSource(DataSource settings, TimeSpan maximumWaitTime)
			: base(settings, maximumWaitTime)
		{
			_logFile = new LogFile(settings.File);
			_logFile.Start();
			CreateFilteredLogFile();
		}

		public override ILogFile LogFile
		{
			get { return _logFile; }
		}
	}
}