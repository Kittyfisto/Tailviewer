using System;
using System.Threading;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class SingleDataSource
		: AbstractDataSource
			, ISingleDataSource
	{
		private readonly ILogSource _originalLogSource;
		private readonly LogSourceProxy _unfilteredLogSource;
		private readonly IPluginDescription _pluginDescription;
		private MultiLineLogSource _multiLineLogSource;

		public SingleDataSource(ILogFileFactory logFileFactory, ITaskScheduler taskScheduler, DataSource settings)
			: this(logFileFactory, taskScheduler, settings, TimeSpan.FromMilliseconds(value: 10))
		{
		}

		public SingleDataSource(ILogFileFactory logFileFactory, ITaskScheduler taskScheduler, DataSource settings,
			TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			if (logFileFactory == null)
				throw new ArgumentNullException(nameof(logFileFactory));

			_originalLogSource = logFileFactory.Open(settings.File, out _pluginDescription);
			_unfilteredLogSource = new LogSourceProxy(TaskScheduler, MaximumWaitTime);
			OnSingleLineChanged();
			OnUnfilteredLogFileChanged();
		}

		public SingleDataSource(ITaskScheduler taskScheduler, DataSource settings, ILogSource unfilteredLogSource,
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