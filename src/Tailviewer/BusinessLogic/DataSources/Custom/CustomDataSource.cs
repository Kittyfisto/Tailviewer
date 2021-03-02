using System;
using System.Threading;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources.Custom
{
	public sealed class CustomDataSource
		: AbstractDataSource
		, ICustomDataSource
	{
		private readonly ILogSource _originalLogFile;
		private readonly LogSourceProxy _unfilteredLogFile;
		private readonly IPluginDescription _pluginDescription;
		private MultiLineLogSource _multiLineLogFile;

		public CustomDataSource(ILogSourceFactoryEx logSourceFactory,
		                        ITaskScheduler taskScheduler,
		                        DataSource settings,
		                        TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			_originalLogFile =
				logSourceFactory.CreateCustom(settings.CustomDataSourceId, settings.CustomDataSourceConfiguration,
				                            out _pluginDescription);
			_unfilteredLogFile = new LogSourceProxy(TaskScheduler, MaximumWaitTime);
			OnSingleLineChanged();
			OnUnfilteredLogFileChanged();
		}

		public override IPluginDescription TranslationPlugin => _pluginDescription;

		public override ILogSource OriginalLogSource => _originalLogFile;

		public override ILogSource UnfilteredLogSource => _unfilteredLogFile;

		protected override void OnSingleLineChanged()
		{
			_multiLineLogFile?.Dispose();

			if (!IsSingleLine)
			{
				_multiLineLogFile = new MultiLineLogSource(TaskScheduler, _originalLogFile, MaximumWaitTime);
				_unfilteredLogFile.InnerLogSource = _multiLineLogFile;
			}
			else
			{
				_unfilteredLogFile.InnerLogSource = _originalLogFile;
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