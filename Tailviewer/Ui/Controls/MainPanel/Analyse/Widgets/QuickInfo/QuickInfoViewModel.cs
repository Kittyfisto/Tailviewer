using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.QuickInfo
{
	public sealed class QuickInfoViewModel
		: INotifyPropertyChanged
	{
		private readonly QuickInfoConfiguration _analyserConfig;
		private readonly Guid _id;
		private readonly DelegateCommand2 _removeCommand;
		private readonly QuickInfoViewConfiguration _viewConfig;
		private bool _isEditing;

		private QuickInfoFormatter _formatter;
		private BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo _result;
		private string _value;

		public QuickInfoViewModel(Guid id, QuickInfoViewConfiguration viewConfig, QuickInfoConfiguration analyserConfig)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));
			if (viewConfig == null)
				throw new ArgumentNullException(nameof(viewConfig));
			if (analyserConfig == null)
				throw new ArgumentNullException(nameof(analyserConfig));

			_id = id;
			_viewConfig = viewConfig;
			_analyserConfig = analyserConfig;
			_removeCommand = new DelegateCommand2(Remove);
			_formatter = new QuickInfoFormatter(analyserConfig);
		}

		public Guid Id => _id;

		public ICommand RemoveCommand => _removeCommand;

		public string Name
		{
			get { return _viewConfig.Name; }
			set
			{
				if (value == _viewConfig.Name)
					return;

				_viewConfig.Name = value;
				EmitPropertyChanged();
			}
		}

		public string FilterValue
		{
			get { return _analyserConfig.Filter.Value; }
			set
			{
				if (value == _analyserConfig.Filter.Value)
					return;

				_analyserConfig.Filter.Value = value;
				EmitPropertyChanged();

				_formatter = new QuickInfoFormatter(_analyserConfig);
			}
		}

		public string Format
		{
			get { return _viewConfig.Format; }
			set
			{
				if (value == _viewConfig.Format)
					return;

				_viewConfig.Format = value;
				EmitPropertyChanged();
				FormatResult();
			}
		}

		public string Value
		{
			get { return _value; }
			private set
			{
				if (value == _value)
					return;

				_value = value;
				EmitPropertyChanged();
			}
		}

		public BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo Result
		{
			get { return _result; }
			set
			{
				if (value == _result)
					return;

				_result = value;
				EmitPropertyChanged();
				FormatResult();
			}
		}

		public bool IsEditing
		{
			get { return _isEditing; }
			set
			{
				if (value == _isEditing)
					return;

				_isEditing = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void FormatResult()
		{
			Value = _formatter.Format(_result, Format);
		}

		private void Remove()
		{
			OnRemoved?.Invoke(this);
		}

		public event Action<QuickInfoViewModel> OnRemoved;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}