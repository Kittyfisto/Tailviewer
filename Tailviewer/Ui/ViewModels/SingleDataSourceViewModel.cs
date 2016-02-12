using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     Represents a data source and is capable
	/// </summary>
	internal sealed class SingleDataSourceViewModel
		: AbstractDataSourceViewModel
	{
		private readonly ICommand _openInExplorerCommand;
		private readonly SingleDataSource _dataSource;
		private readonly string _fileName;
		private readonly string _folder;

		public SingleDataSourceViewModel(SingleDataSource dataSource)
			: base(dataSource)
		{
			_dataSource = dataSource;
			_fileName = Path.GetFileName(dataSource.FullFileName);
			_folder = Path.GetDirectoryName(dataSource.FullFileName);
			_openInExplorerCommand = new DelegateCommand(OpenInExplorer);
			Update();
		}

		public override string ToString()
		{
			return DisplayName;
		}

		public override ICommand OpenInExplorerCommand
		{
			get { return _openInExplorerCommand; }
		}

		public override string DisplayName
		{
			get { return _fileName; }
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public string Folder
		{
			get { return _folder; }
		}

		public string FullName
		{
			get { return _dataSource.FullFileName; }
		}

		private void OpenInExplorer()
		{
			string argument = string.Format(@"/select, {0}", _dataSource.FullFileName);
			Process.Start("explorer.exe", argument);
		}
	}
}