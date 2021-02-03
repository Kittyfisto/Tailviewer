using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Settings;
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
			var socket = new UdpSocket(ParseEndPoint(config.Address));
			var defaultEncoding = serviceContainer.TryRetrieve<ILogFileSettings>()?.DefaultEncoding;
			var overwrittenEncoding = serviceContainer.TryRetrieve<Encoding>();
			var encoding = overwrittenEncoding ?? defaultEncoding ?? Encoding.UTF8;
			return new UdpLogFile(serviceContainer.Retrieve<ITaskScheduler>(),
			                      encoding,
			                      socket);
		}

		private IPEndPoint ParseEndPoint(string configAddress)
		{
			var idx = configAddress.LastIndexOf(":");
			if (idx == -1)
				throw new ArgumentException($"Invalid IPEndPoint '{configAddress}': Expected a form of 'address:port'");

			var address = IPAddress.Parse(configAddress.Substring(0, idx));
			var port = int.Parse(configAddress.Substring(idx + 1));
			return new IPEndPoint(address, port);
		}

		#endregion
	}
}