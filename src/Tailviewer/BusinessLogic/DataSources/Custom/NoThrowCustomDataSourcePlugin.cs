using System;
using System.Reflection;
using System.Windows;
using log4net;
using Tailviewer.Core.Sources;
using Tailviewer.Plugins;
using Tailviewer.Ui;

namespace Tailviewer.BusinessLogic.DataSources.Custom
{
	public sealed class NoThrowCustomDataSourcePlugin
		: ICustomDataSourcePlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string _name;
		private readonly ICustomDataSourcePlugin _inner;
		private readonly CustomDataSourceId _id;

		public NoThrowCustomDataSourcePlugin(ICustomDataSourcePlugin inner)
		{
			_inner = inner;

			try
			{
				_name = inner.DisplayName;
				_id = inner.Id;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				_name = inner.GetType().Name;
				_id = null;
			}
		}

		#region Implementation of ICustomDataSourcePlugin

		public string DisplayName => _name;

		public CustomDataSourceId Id => _id;

		public ICustomDataSourceConfiguration CreateConfiguration(IServiceContainer serviceContainer)
		{
			return new NoThrowCustomDataSourceConfiguration(_inner, serviceContainer);
		}

		public ICustomDataSourceViewModel CreateViewModel(IServiceContainer serviceContainer,
		                                                  ICustomDataSourceConfiguration configuration)
		{
			var actualConfig = ((NoThrowCustomDataSourceConfiguration) configuration).Inner;
			try
			{
				return _inner.CreateViewModel(serviceContainer, actualConfig);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}

		public FrameworkElement CreateConfigurationControl(IServiceContainer serviceContainer, ICustomDataSourceViewModel viewModel)
		{
			try
			{
				return _inner.CreateConfigurationControl(serviceContainer, viewModel);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}

		public ILogSource CreateLogSource(IServiceContainer serviceContainer, ICustomDataSourceConfiguration configuration)
		{
			var actualConfig = ((NoThrowCustomDataSourceConfiguration) configuration).Inner;
			try
			{
				var actualLogFile = _inner.CreateLogSource(serviceContainer, actualConfig);
				return new NoThrowLogSource(actualLogFile, "TODO");
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new EmptyLogSource();
			}
		}

		#endregion
	}
} 