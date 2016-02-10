using System.Collections.ObjectModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MergedDataSourceViewModel
		: AbstractDataSourceViewModel
	{
		private readonly ObservableCollection<IDataSourceViewModel> _dataSources;

		public MergedDataSourceViewModel(IDataSource dataSource)
			: base(dataSource)
		{
			_dataSources = new ObservableCollection<IDataSourceViewModel>();
		}

		public void AddChild(IDataSourceViewModel dataSource)
		{
			_dataSources.Remove(dataSource);
		}

		public void RemoveChild(IDataSourceViewModel dataSource)
		{
			_dataSources.Remove(dataSource);
		}

		public override ICommand OpenInExplorerCommand
		{
			get { return null; }
		}

		public override string DisplayName
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}