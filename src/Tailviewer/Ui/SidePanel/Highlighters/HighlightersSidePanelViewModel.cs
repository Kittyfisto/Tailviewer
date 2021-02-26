using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic.Highlighters;

namespace Tailviewer.Ui.SidePanel.Highlighters
{
	/// <summary>
	///     Responsible for presenting the list of highlighters to the user who is able
	///     to add new, change existing ones and delete them.
	/// </summary>
	public sealed class HighlightersSidePanelViewModel
		: AbstractSidePanelViewModel
	{
		private readonly DelegateCommand2 _addCommand;
		private readonly IHighlighters _highlighters;
		private readonly ObservableCollection<HighlighterViewModel> _highlighterViewModels;

		public HighlightersSidePanelViewModel(IHighlighters highlighters)
		{
			_highlighters = highlighters;
			_highlighterViewModels = new ObservableCollection<HighlighterViewModel>();
			foreach (var highlighter in _highlighters.Highlighters)
			{
				AddHighlighterViewModel(highlighter);
			}

			_addCommand = new DelegateCommand2(Add);
		}

		public IEnumerable<HighlighterViewModel> Highlighters
		{
			get { return _highlighterViewModels; }
		}

		public ICommand AddCommand
		{
			get { return _addCommand; }
		}

		private void Add()
		{
			var highlighter = _highlighters.AddHighlighter();
			AddHighlighterViewModel(highlighter);
		}

		private void AddHighlighterViewModel(Highlighter highlighter)
		{
			var viewModel = new HighlighterViewModel(highlighter, OnRemove);
			_highlighterViewModels.Add(viewModel);
		}

		private void OnRemove(HighlighterViewModel viewModel)
		{
			_highlighterViewModels.Remove(viewModel);
		}

		#region Overrides of AbstractSidePanelViewModel

		public override Geometry Icon
		{
			get { return Icons.Marker; }
		}

		public override string Id
		{
			get { return "highlighters"; }
		}

		public override void Update()
		{
		}

		#endregion
	}
}