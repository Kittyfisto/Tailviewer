using System;
using System.Threading;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class FileDataSource
		: AbstractDataSource
		, IFileDataSource
	{
		private readonly ILogFile _originalLogFile;
		private readonly LogFileProxy _unfilteredLogFile;
		private readonly IPluginDescription _pluginDescription;
		private MultiLineLogFile _multiLineLogFile;

		public FileDataSource(ILogFileFactory logFileFactory, ITaskScheduler taskScheduler, DataSource settings)
			: this(logFileFactory, taskScheduler, settings, TimeSpan.FromMilliseconds(value: 10))
		{
		}

		public FileDataSource(ILogFileFactory logFileFactory, ITaskScheduler taskScheduler, DataSource settings,
			TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			if (logFileFactory == null)
				throw new ArgumentNullException(nameof(logFileFactory));

			_originalLogFile = logFileFactory.Open(settings.File, out _pluginDescription);
			_unfilteredLogFile = new LogFileProxy(TaskScheduler, MaximumWaitTime);
			OnSingleLineChanged();
			OnUnfilteredLogFileChanged();
		}

		public FileDataSource(ITaskScheduler taskScheduler, DataSource settings, ILogFile unfilteredLogFile,
			TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			if (unfilteredLogFile == null)
				throw new ArgumentNullException(nameof(unfilteredLogFile));

			_originalLogFile = unfilteredLogFile;
			_unfilteredLogFile = new LogFileProxy(TaskScheduler, MaximumWaitTime);
			OnSingleLineChanged();
			OnUnfilteredLogFileChanged();
		}

		public override IPluginDescription TranslationPlugin => _pluginDescription;

		public override ILogFile OriginalLogFile => _originalLogFile;

		public override ILogFile UnfilteredLogFile => _unfilteredLogFile;

		protected override void OnSingleLineChanged()
		{
			_multiLineLogFile?.Dispose();

			if (!IsSingleLine)
			{
				_multiLineLogFile = new MultiLineLogFile(TaskScheduler, _originalLogFile, MaximumWaitTime);
				_unfilteredLogFile.InnerLogFile = _multiLineLogFile;
			}
			else
			{
				_unfilteredLogFile.InnerLogFile = _originalLogFile;
			}
		}

		protected override void DisposeAdditional()
		{
			_originalLogFile?.Dispose();
			_unfilteredLogFile?.Dispose();
			_multiLineLogFile?.Dispose();
		}
	}
}