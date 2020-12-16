using System;
using System.Threading;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources.Custom
{
	public sealed class CustomDataSource
		: AbstractDataSource
		, ICustomDataSource
	{
		private readonly ILogFile _originalLogFile;
		private readonly LogFileProxy _unfilteredLogFile;
		private readonly IPluginDescription _pluginDescription;
		private MultiLineLogFile _multiLineLogFile;

		public CustomDataSource(ILogFileFactory logFileFactory,
		                        ITaskScheduler taskScheduler,
		                        DataSource settings,
		                        TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			_originalLogFile =
				logFileFactory.CreateCustom(settings.CustomDataSourceId, settings.CustomDataSourceConfiguration,
				                            out _pluginDescription);
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

		#region Implementation of ICustomDataSource

		public ICustomDataSourceConfiguration Configuration => Settings.CustomDataSourceConfiguration;

		#endregion
	}
}