using System;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class SingleDataSource
		: AbstractDataSource
	{
		private readonly ILogFile _unfilteredLogFile;

		public SingleDataSource(ITaskScheduler taskScheduler, DataSource settings)
			: this(taskScheduler, settings, TimeSpan.FromMilliseconds(10))
		{
		}

		public SingleDataSource(ITaskScheduler taskScheduler, DataSource settings, TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			var logFile = new LogFile(taskScheduler, settings.File);
			_unfilteredLogFile = logFile;
			CreateFilteredLogFile();
		}

		public SingleDataSource(ITaskScheduler taskScheduler, DataSource settings, ILogFile unfilteredLogFile, TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
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