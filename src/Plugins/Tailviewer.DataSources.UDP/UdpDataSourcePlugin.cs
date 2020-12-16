using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Ui;

namespace Tailviewer.DataSources.UDP
{
	/// <summary>
	/// </summary>
	public sealed class UdpDataSourcePlugin
		: ICustomDataSourcePlugin
	{
		#region Implementation of ICustomDataSourcePlugin

		public string DisplayName
		{
			get { return "UDP"; }
		}

		public CustomDataSourceId Id
		{
			get { return new CustomDataSourceId("Tailviewer.DataSources.UDP.UdpDataSourcePlugin"); }
		}

		public ICustomDataSourceConfiguration CreateConfiguration(IServiceContainer serviceContainer)
		{
			return new UdpCustomDataSourceConfiguration();
		}

		public ICustomDataSourceViewModel CreateViewModel(IServiceContainer serviceContainer,
		                                                  ICustomDataSourceConfiguration configuration)
		{
			return new UdpDataSourceViewModel((UdpCustomDataSourceConfiguration)configuration);
		}

		public FrameworkElement CreateConfigurationControl(IServiceContainer serviceContainer,
		                                                   ICustomDataSourceViewModel
			                                                   viewModel)
		{
			return new TextBlock {Text = "UDP Test"};
		}

		public ILogFile CreateLogFile(IServiceContainer serviceContainer, ICustomDataSourceConfiguration configuration)
		{
			var config = (UdpCustomDataSourceConfiguration) configuration;
			return new UdpLogFile(serviceContainer.Retrieve<ITaskScheduler>(),
			                      config.Address);
		}

		#endregion
	}
}