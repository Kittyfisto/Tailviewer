using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	internal sealed class SingleDataSource
		: AbstractDataSource
	{
		private readonly ILogFile _unfilteredLogFile;

		public SingleDataSource(DataSource settings)
			: this(settings, TimeSpan.FromMilliseconds(100))
		{
		}

		public SingleDataSource(DataSource settings, TimeSpan maximumWaitTime)
			: base(settings, maximumWaitTime)
		{
			var logFile = new LogFile(settings.File);
			logFile.Start();
			_unfilteredLogFile = logFile;
			CreateFilteredLogFile();
		}

		public SingleDataSource(DataSource settings, ILogFile unfilteredLogFile, TimeSpan maximumWaitTime)
			: base(settings, maximumWaitTime)
		{
			_unfilteredLogFile = unfilteredLogFile;
			CreateFilteredLogFile();
		}

		public override ILogFile UnfilteredLogFile
		{
			get { return _unfilteredLogFile; }
		}
	}
}