using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class ExcludeAllInGroupViewModel
		: IContextMenuViewModel
	{
		private readonly IMergedDataSourceViewModel _viewModel;
		private readonly DelegateCommand2 _command;

		public ExcludeAllInGroupViewModel(IMergedDataSourceViewModel viewModel)
		{
			_viewModel = viewModel;
			_command = new DelegateCommand2(OnExcludeAll);
		}

		private void OnExcludeAll()
		{
			foreach (var dataSource in _viewModel.Observable)
			{
				if (dataSource is ISingleDataSourceViewModel singleDataSource)
					singleDataSource.ExcludeFromParent = true;
			}
		}

		#region Implementation of IContextMenuViewModel

		public string Header
		{
			get { return "Exclude all"; }
		}

		public ICommand Command => _command;

		#endregion
	}
}