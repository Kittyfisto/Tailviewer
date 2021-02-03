using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.Ui;

namespace Tailviewer.DataSources.UDP
{
	public sealed class UdpDataSourceViewModel
		: ICustomDataSourceViewModel
		, INotifyPropertyChanged
	{
		private readonly UdpCustomDataSourceConfiguration _configuration;

		public UdpDataSourceViewModel(UdpCustomDataSourceConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string Address
		{
			get { return _configuration.Address; }
			set
			{
				if (value == _configuration.Address)
					return;

				_configuration.Address = value;
				EmitPropertyChanged();
			}
		}

		#region Implementation of ICustomDataSourceConfigurationViewModel


		public event Action<ICustomDataSourceViewModel> RequestStore;

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}