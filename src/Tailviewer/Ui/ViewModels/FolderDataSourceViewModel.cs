using System.Windows.Input;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class FolderDataSourceViewModel
		: AbstractDataSourceViewModel
		, IDataSourceViewModel
	{
		private readonly IFolderDataSource _dataSource;

		public FolderDataSourceViewModel(IFolderDataSource folder, IActionCenter actionCenter)
			: base(folder)
		{
			_dataSource = folder;
		}

		#region Overrides of AbstractDataSourceViewModel

		public override ICommand OpenInExplorerCommand => null;

		public override string DisplayName { get; set; }

		public override bool CanBeRenamed => false;

		public override string DataSourceOrigin => _dataSource.LogFileFolderPath;

		#endregion
	}
}