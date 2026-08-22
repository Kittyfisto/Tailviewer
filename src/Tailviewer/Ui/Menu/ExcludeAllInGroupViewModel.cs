using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Ui.DataSourceTree;

using Tailviewer.Localization;

namespace Tailviewer.Ui.Menu
{
	public sealed class ExcludeAllInGroupViewModel
		: IMenuViewModel
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
			get { return Strings.ExcludeAll; }
		}

		public string ToolTip => null;

		public string Shortcut => null;

		public Geometry Icon => null;

		public ICommand Command => _command;

		public bool IsCheckable => false;

		public bool IsChecked { get; set; }

		public IEnumerable<IMenuViewModel> Children => null;

		#endregion
	}
}