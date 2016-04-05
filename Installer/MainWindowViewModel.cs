using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;

namespace Installer
{
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private bool _agreeToLicense;

		public string AppVersion
		{
			get { throw new NotImplementedException(); }
		}

		public Size InstallationSize
		{
			get { throw new NotImplementedException(); }
		}

		public string InstallationPath
		{
			get { throw new NotImplementedException(); }
		}

		public ICommand BrowseCommand
		{
			get { throw new NotImplementedException(); }
		}

		public bool AgreeToLicense
		{
			get { return _agreeToLicense; }
			set
			{
				if (value == _agreeToLicense)
					return;

				_agreeToLicense = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}