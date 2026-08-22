using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Collections;
using Tailviewer.Ui.DataSourceTree;

using Tailviewer.Localization;

namespace Tailviewer.Ui.Menu
{
	/// <summary>
	///     Responsible for building the (dynamic) file menu.
	/// </summary>
	public sealed class FileMenuViewModel
		: IMenu
	{
		private readonly ObservableCollection<IMenuViewModel> _openItems;
		private readonly ObservableCollection<IMenuViewModel> _closeItems;
		private readonly ObservableCollection<IMenuViewModel> _pluginsItems;
		private readonly ObservableCollection<IMenuViewModel> _settingsItems;
		private readonly ObservableCollection<IMenuViewModel> _exitItems;
		private readonly ObservableCollection<IMenuViewModel> _newCustomMenuViewModels;
		private readonly CompoundObservableCollection<IMenuViewModel> _fileMenuItems;
		private readonly int _fileMenuDataSourceInsertionIndex;
		private readonly int _fileMenuMinimumCount;

		public FileMenuViewModel(ICommand addDataSourceFromFile,
		                         ICommand addDataSourceFromFolder,
		                         ICommand closeCurrentDataSource,
		                         ICommand closeAllDataSources,
		                         ICommand showPlugins,
		                         ICommand showSettings,
		                         ICommand exitCommand)
		{
			_newCustomMenuViewModels = new ObservableCollection<IMenuViewModel>
			{

			};

			var openFileMenuViewModels = new[]
			{
				new CommandMenuViewModel(addDataSourceFromFile)
				{
					Header = Strings.MenuFile,
					Icon = Icons.FileOutline,
				},
				new CommandMenuViewModel(addDataSourceFromFolder)
				{
					Header = Strings.MenuOpenFolder,
					Icon = Icons.FolderOpen
				}
			};

			_openItems = new ObservableCollection<IMenuViewModel>
			{
				new ParentMenuViewModel(_newCustomMenuViewModels)
				{
					Header = Strings.MenuNew
				},
				new ParentMenuViewModel(openFileMenuViewModels)
				{
					Header = Strings.MenuOpen
				}
			};

			// We instruct the special collection to place a null value as a separator in between different
			// collections and then instruct the style (in xaml) to place a separator whenever a null value
			// is encountered.
			_fileMenuItems = new CompoundObservableCollection<IMenuViewModel>(true, null) {_openItems};
			_fileMenuDataSourceInsertionIndex = _fileMenuItems.ChildCollectionCount;
			_closeItems = new ObservableCollection<IMenuViewModel>
			{
				new CommandMenuViewModel(closeCurrentDataSource)
				{
					Header = Strings.MenuClose,
				},
				new CommandMenuViewModel(closeAllDataSources)
				{
					Header = Strings.MenuCloseAll,
				},
			};
			_fileMenuItems.Add(_closeItems);
			_pluginsItems = new ObservableCollection<IMenuViewModel>
			{
				new CommandMenuViewModel(showPlugins)
				{
					Header = Strings.MenuPlugins,
					Icon = Icons.PuzzleOutline
				}
			};
			_fileMenuItems.Add(_pluginsItems);
			_settingsItems = new ObservableCollection<IMenuViewModel>
			{
				new CommandMenuViewModel(showSettings)
				{
					Header = Strings.MenuSettings,
					Icon = Icons.CogOutline
				}
			};
			_fileMenuItems.Add(_settingsItems);
			_exitItems = new ObservableCollection<IMenuViewModel>
			{
				new CommandMenuViewModel(new KeyBindingCommand(exitCommand)
				{
					GestureModifier = ModifierKeys.Alt,
					GestureKey = Key.F4
				})
				{
					Header = Strings.MenuExit
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

		public bool HasItems => true;

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}