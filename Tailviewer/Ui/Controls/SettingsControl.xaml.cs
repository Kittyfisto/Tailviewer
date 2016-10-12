using System.ComponentModel;
using System.Windows;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	/// <summary>
	///     Interaction logic for SettingsControl.xaml
	/// </summary>
	public partial class SettingsControl
	{
		public SettingsControl()
		{
			InitializeComponent();

			DataContextChanged += OnDataContextChanged;
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			var old = args.OldValue as SettingsViewModel;
			if (old != null)
			{
				old.PropertyChanged -= DataContextOnPropertyChanged;
				PART_ProxyPassword.Password = null;
			}

			var @new = args.NewValue as SettingsViewModel;
			if (@new != null)
			{
				@new.PropertyChanged += DataContextOnPropertyChanged;
				PART_ProxyPassword.Password = @new.ProxyPassword;
			}
		}

		private void DataContextOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "ProxyPassword":
					PART_ProxyPassword.Password = ((SettingsViewModel) DataContext).ProxyPassword;
					break;
			}
		}

		private void OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			((SettingsViewModel) DataContext).ProxyPassword = PART_ProxyPassword.Password;
		}
	}
}