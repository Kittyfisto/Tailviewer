using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class IncludeAllInGroupViewModel
		: IContextMenuViewModel
	{
		private readonly IMergedDataSourceViewModel _viewModel;
		private readonly DelegateCommand2 _command;

		public IncludeAllInGroupViewModel(IMergedDataSourceViewModel viewModel)
		{
			_viewModel = viewModel;
			_command = new DelegateCommand2(OnIncludeAll);
		}

		private void OnIncludeAll()
		{
			foreach (var dataSource in _viewModel.Observable)
			{
				if (dataSource is ISingleDataSourceViewModel singleDataSource)
					singleDataSource.ExcludeFromParent = false;
			}
		}

		#region Implementation of IContextMenuViewModel

		public string Header
		{
			get { return "Include all"; }
		}

		public ICommand Command => _command;

		#endregion
	}
}