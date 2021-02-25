using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Metrolib;

namespace Tailviewer.Ui.ViewModels.ContextMenu
{
	public sealed class IncludeAllInGroupViewModel
		: IMenuViewModel
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

		public string ToolTip => null;

		public Geometry Icon => null;

		public ICommand Command => _command;

		public bool IsCheckable => false;

		public bool IsChecked
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public IEnumerable<IMenuViewModel> Children => null;

		#endregion
	}
}