using System;
using System.Threading;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class FileDataSource
		: AbstractDataSource
		, IFileDataSource
	{
		private readonly ILogSource _originalLogSource;
		private readonly LogSourceProxy _unfilteredLogSource;
		private readonly IPluginDescription _pluginDescription;
		private MultiLineLogSource _multiLineLogSource;

		public FileDataSource(ILogSourceFactory logSourceFactory, ITaskScheduler taskScheduler, DataSource settings)
			: this(logSourceFactory, taskScheduler, settings, TimeSpan.FromMilliseconds(value: 10))
		{
		}

		public FileDataSource(ILogSourceFactory logSourceFactory, ITaskScheduler taskScheduler, DataSource settings,
			TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			if (logSourceFactory == null)
				throw new ArgumentNullException(nameof(logSourceFactory));

			_originalLogSource = logSourceFactory.Open(settings.File);
			_unfilteredLogSource = new LogSourceProxy(TaskScheduler, MaximumWaitTime);
			OnSingleLineChanged();
			OnUnfilteredLogFileChanged();
		}

		public FileDataSource(ITaskScheduler taskScheduler, DataSource settings, ILogSource unfilteredLogSource,
			TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			if (unfilteredLogSource == null)
				throw new ArgumentNullException(nameof(unfilteredLogSource));

			_originalLogSource = unfilteredLogSource;
			_unfilteredLogSource = new LogSourceProxy(TaskScheduler, MaximumWaitTime);
			OnSingleLineChanged();
			OnUnfilteredLogFileChanged();
		}

		public override IPluginDescription TranslationPlugin => _pluginDescription;

		public override ILogSource OriginalLogSource => _originalLogSource;

		public override ILogSource UnfilteredLogSource => _unfilteredLogSource;

		protected override void OnSingleLineChanged()
		{
			_multiLineLogSource?.Dispose();

			if (!IsSingleLine)
			{
				_multiLineLogSource = new MultiLineLogSource(TaskScheduler, _originalLogSource, MaximumWaitTime);
				_unfilteredLogSource.InnerLogSource = _multiLineLogSource;
			}
			else
			{
				_unfilteredLogSource.InnerLogSource = _originalLogSource;
			}
		}

		protected override void DisposeAdditional()
		{
			_originalLogSource?.Dispose();
			_unfilteredLogSource?.Dispose();
			_multiLineLogSource?.Dispose();
		}
	}
}