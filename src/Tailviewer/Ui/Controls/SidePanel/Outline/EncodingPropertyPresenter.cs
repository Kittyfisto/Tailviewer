using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using log4net;
using Tailviewer.Ui.Controls.MainPanel.Settings;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	public sealed class EncodingPropertyPresenter
		: IPropertyPresenter
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string _displayName;
		private readonly ComboBox _comboBox;
		private readonly List<EncodingViewModel> _encodings;

		public EncodingPropertyPresenter(string displayName)
		{
			_displayName = displayName;
			_encodings = new List<EncodingViewModel>(SettingsMainPanelViewModel.Encodings);

			_comboBox = new ComboBox
			{
				ItemsSource = _encodings
			};
		}

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Implementation of IPropertyPresenter

		public string DisplayName => _displayName;

		public object Value
		{
			get
			{
				return _comboBox;
			}
		}

		public void Update(object newValue)
		{
			if (newValue == null)
			{
				_comboBox.SelectedValue = null;
			}
			else
			{
				var viewModel = _encodings.FirstOrDefault(x => Equals(x.Encoding, newValue));
				if (viewModel != null)
				{
					_comboBox.SelectedValue = viewModel;
				}
				else
				{
					Log.WarnFormat("No model found for encoding: {0}", newValue);
				}
			}
		}

		public event Action<object> OnValueChanged;

		#endregion

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void EmitOnValueChanged(object obj)
		{
			OnValueChanged?.Invoke(obj);
		}
	}
}