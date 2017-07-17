using System;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class SingleDataSource
		: AbstractDataSource
	{
		private readonly ILogFile _unfilteredLogFile;

		public SingleDataSource(ILogFileFactory logFileFactory, ITaskScheduler taskScheduler, DataSource settings)
			: this(logFileFactory, taskScheduler, settings, TimeSpan.FromMilliseconds(10))
		{
		}

		public SingleDataSource(ILogFileFactory logFileFactory, ITaskScheduler taskScheduler, DataSource settings, TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			if (logFileFactory == null)
				throw new ArgumentNullException(nameof(logFileFactory));

			var logFile = logFileFactory.Open(settings.File);
			_unfilteredLogFile = logFile;
			OnUnfilteredLogFileChanged();
		}

		public SingleDataSource(ITaskScheduler taskScheduler, DataSource settings, ILogFile unfilteredLogFile, TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			if (unfilteredLogFile == null)
				throw new ArgumentNullException(nameof(unfilteredLogFile));

			_unfilteredLogFile = unfilteredLogFile;
			OnUnfilteredLogFileChanged();
		}

		public override ILogFile UnfilteredLogFile => _unfilteredLogFile;
	}
}