using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Ui.ViewModels.ContextMenu;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     Responsible for building the (dynamic) file menu.
	/// </summary>
	public sealed class FileMenu
	{
		private readonly ObservableCollection<IMenuViewModel> _openItems;
		private readonly ObservableCollection<IMenuViewModel> _exitItems;
		private readonly ObservableCollection<IMenuViewModel> _newCustomMenuViewModels;
		private readonly CompoundObservableCollection<IMenuViewModel> _fileMenuItems;
		private readonly int _fileMenuDataSourceInsertionIndex;
		private readonly int _fileMenuMinimumCount;

		public FileMenu(ICommand addDataSourceFromFile,
		                ICommand addDataSourceFromFolder,
		                ICommand exitCommand)
		{
			_newCustomMenuViewModels = new ObservableCollection<IMenuViewModel>
			{

			};

			var openFileMenuViewModels = new[]
			{
				new CommandMenuViewModel(addDataSourceFromFile)
				{
					Header = "_File",
					Icon = Icons.File,
				},
				new CommandMenuViewModel(addDataSourceFromFolder)
				{
					Header = "Fol_der",
					Icon = Icons.FolderOpen
				}
			};

			_openItems = new ObservableCollection<IMenuViewModel>
			{
				new ParentMenuViewModel(_newCustomMenuViewModels)
				{
					Header = "_New"
				},
				new ParentMenuViewModel(openFileMenuViewModels)
				{
					Header = "_Open"
				}
			};

			_fileMenuItems = new CompoundObservableCollection<IMenuViewModel>(null) {_openItems};
			_fileMenuDataSourceInsertionIndex = _fileMenuItems.ChildCollectionCount;
			_exitItems = new ObservableCollection<IMenuViewModel>
			{
				new CommandMenuViewModel(exitCommand)
				{
					Header = "E_xit"
				}
			};
			_fileMenuItems.Add(_exitItems);
			_fileMenuMinimumCount = _fileMenuItems.ChildCollectionCount;
		}

		public IDataSourceViewModel CurrentDataSource
		{
			set
			{
				var dataSourceFileMenuItems = value?.FileMenuItems;
				if (_fileMenuItems.ChildCollectionCount == _fileMenuMinimumCount)
				{
					_fileMenuItems.Insert(_fileMenuDataSourceInsertionIndex, dataSourceFileMenuItems);
				}
				else
				{
					_fileMenuItems.RemoveAt(_fileMenuDataSourceInsertionIndex);
					_fileMenuItems.Insert(_fileMenuDataSourceInsertionIndex, dataSourceFileMenuItems);
				}
			}
		}

		public IEnumerable<IMenuViewModel> Items => _fileMenuItems;
	}
}