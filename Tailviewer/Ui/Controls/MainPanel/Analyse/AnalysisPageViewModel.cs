using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a single page of an analysis.
	///     A page consists of zero or more <see cref="IWidgetViewModel" />s which are
	///     presented using a <see cref="IWidgetLayoutViewModel" />.
	///     The layout of a page can be changed through <see cref="PageLayout" />.
	/// </summary>
	public sealed class AnalysisPageViewModel
		: INotifyPropertyChanged
	{
		private readonly DelegateCommand _deletePageCommand;
		private readonly List<IWidgetViewModel> _widgets;
		private bool _canBeDeleted;
		private IWidgetLayoutViewModel _layout;
		private string _name;
		private PageLayout _pageLayout;

		public AnalysisPageViewModel()
		{
			_name = "New Page";
			_deletePageCommand = new DelegateCommand(DeletePage, () => _canBeDeleted);
			_widgets = new List<IWidgetViewModel>();

			PageLayout = PageLayout.WrapHorizontal;
			Add(new TutorialWidgetViewModel());
		}

		/// <summary>
		///     The layout used to display widgets.
		/// </summary>
		public PageLayout PageLayout
		{
			get { return _pageLayout; }
			set
			{
				if (value == _pageLayout)
					return;

				_pageLayout = value;
				EmitPropertyChanged();

				switch (value)
				{
					case PageLayout.None:
						Layout = null;
						break;

					case PageLayout.WrapHorizontal:
						Layout = new HorizontalWidgetLayoutViewModel();
						break;
				}
			}
		}

		public IWidgetLayoutViewModel Layout
		{
			get { return _layout; }
			private set
			{
				if (value == _layout)
					return;

				_layout = value;
				EmitPropertyChanged();

				if (_layout != null)
					foreach (var widget in _widgets)
						_layout.Add(widget);
			}
		}

		public void Add(IWidgetViewModel widget)
		{
			_widgets.Add(widget);
			_layout?.Add(widget);
			widget.OnDelete += WidgetOnDelete;
		}

		private void WidgetOnDelete(IWidgetViewModel widget)
		{
			Remove(widget);
		}

		public void Remove(IWidgetViewModel widget)
		{
			_widgets.Remove(widget);
			_layout?.Remove(widget);
			widget.OnDelete -= WidgetOnDelete;
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name)
					return;
				_name = value;
				EmitPropertyChanged();
			}
		}

		public ICommand DeletePageCommand => _deletePageCommand;

		public bool CanBeDeleted
		{
			get { return _canBeDeleted; }
			set
			{
				_canBeDeleted = value;
				_deletePageCommand.RaiseCanExecuteChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void DeletePage()
		{
			OnDelete?.Invoke(this);
		}

		public event Action<AnalysisPageViewModel> OnDelete;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}