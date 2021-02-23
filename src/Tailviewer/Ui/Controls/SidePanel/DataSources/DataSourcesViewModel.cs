using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.DataSources.Custom;
using Tailviewer.Plugins;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.SidePanel.DataSources
{
	public sealed class DataSourcesViewModel
		: AbstractSidePanelViewModel
	{
		private readonly List<IDataSourceViewModel> _allDataSourceViewModels;
		private readonly IDataSources _dataSources;
		private readonly ObservableCollection<IDataSourceViewModel> _observable;
		private readonly IActionCenter _actionCenter;
		private readonly IApplicationSettings _settings;
		private readonly ICommand _addDataSourceFromFileCommand;
		private readonly ICommand _addDataSourceFromFolderCommand;
		private readonly IReadOnlyList<AddCustomDataSourceViewModel> _customDataSources;
		private IDataSourceViewModel _selectedItem;
		private bool _isPinned;

		public DataSourcesViewModel(IApplicationSettings settings, IDataSources dataSources, IActionCenter actionCenter)
		{
			_actionCenter = actionCenter ?? throw new ArgumentNullException(nameof(actionCenter));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_observable = new ObservableCollection<IDataSourceViewModel>();
			_allDataSourceViewModels = new List<IDataSourceViewModel>();
			_addDataSourceFromFileCommand = new DelegateCommand(AddDataSourceFromFile);
			_addDataSourceFromFolderCommand = new DelegateCommand(AddDataSourceFromFolder);
			_dataSources = dataSources ?? throw new ArgumentNullException(nameof(dataSources));
			foreach (IDataSource dataSource in dataSources.Sources)
			{
				if (dataSource.ParentId == DataSourceId.Empty)
				{
					Add(dataSource);
				}
			}

			foreach (IDataSource dataSource in dataSources.Sources)
			{
				DataSourceId parentId = dataSource.ParentId;
				if (parentId != DataSourceId.Empty)
				{
					IDataSourceViewModel parent = _observable.First(x => x.DataSource.Id == parentId);
					var group = (MergedDataSourceViewModel) parent;
					IDataSourceViewModel viewModel = CreateViewModel(dataSource);
					group.AddChild(viewModel);
				}
			}

			_customDataSources =
				_dataSources.CustomDataSources.Select(x => new AddCustomDataSourceViewModel(x.DisplayName, () => AddCustomDataSource(x.Id))).ToList();

			UpdateTooltip();
			PropertyChanged += OnPropertyChanged;
		}

		private void UpdateTooltip()
		{
			Tooltip = IsSelected
				? "Hide the list of data sources"
				: "Show the list of data sources";
		}

		private void AddCustomDataSource(CustomDataSourceId id)
		{
			var dataSource = _dataSources.AddCustom(id);
			Add(dataSource);
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(IsSelected):
					UpdateTooltip();
					break;
			}
		}

		public ICommand AddDataSourceFromFileCommand => _addDataSourceFromFileCommand;

		public ICommand AddDataSourceFromFolderCommand => _addDataSourceFromFolderCommand;

		public IEnumerable<AddCustomDataSourceViewModel> CustomDataSources => _customDataSources;

		public bool IsPinned
		{
			get { return _isPinned; }
			set
			{
				if (value == _isPinned)
					return;
				_isPinned = value;
				_settings.DataSources.IsPinned = value;
				EmitPropertyChanged();
			}
		}

		public IDataSourceViewModel SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				if (value == _selectedItem)
					return;

				foreach (IDataSourceViewModel dataSource in _allDataSourceViewModels)
				{
					dataSource.IsVisible = false;
				}

				_selectedItem = value;
				if (value != null)
				{
					_settings.DataSources.SelectedItem = value.DataSource.Id;
					value.IsVisible = true;
					QuickInfo = value.DisplayName;
				}
				else
				{
					_settings.DataSources.SelectedItem = DataSourceId.Empty;
					QuickInfo = null;
				}

				EmitPropertyChanged();
			}
		}

		public IReadOnlyList<IDataSourceViewModel> DataSources => _allDataSourceViewModels;

		public ObservableCollection<IDataSourceViewModel> Observable => _observable;

		public override void Update()
		{
			foreach (IDataSourceViewModel dataSource in _allDataSourceViewModels)
			{
				dataSource.Update();
			}
		}

		public IDataSourceViewModel GetOrAddFile(string fileName)
		{
			string fullName = Path.GetFullPath(fileName);
			IDataSourceViewModel viewModel =
				_observable.FirstOrDefault(x => Represents(x, fullName));
			if (viewModel == null)
			{
				IDataSource dataSource = _dataSources.AddFile(fileName);
				viewModel = Add(dataSource);
				_settings.SaveAsync();

				if (_observable.Count == 1)
				{
					SelectedItem = viewModel;
				}
			}

			return viewModel;
		}

		public IDataSourceViewModel GetOrAddFolder(string folder)
		{
			string fullPath = Path.GetFullPath(folder);
			IDataSourceViewModel viewModel =
				_observable.FirstOrDefault(x => Represents(x, fullPath));

			if (viewModel == null)
			{
				IDataSource dataSource = _dataSources.AddFolder(fullPath);
				viewModel = Add(dataSource);
				_settings.SaveAsync();

				if (_observable.Count == 1)
				{
					SelectedItem = viewModel;
				}
			}

			return viewModel;
		}

		public override Geometry Icon => Icons.Database;

		public override string Id => "datasources";

		[Pure]
		public IDataSourceViewModel TryGet(DataSourceId id)
		{
			var viewModel = _observable.FirstOrDefault(x => x.DataSource.Id == id);
			return viewModel;
		}

		private void AddDataSourceFromFile()
		{
			var dlg = new OpenFileDialog
			{
				Title = "Select log file to open",
				DefaultExt = ".log",
				Filter = "Log Files (*.log)|*.log|Txt Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
				Multiselect = true
			};

			if (dlg.ShowDialog() == true)
			{
				string[] selectedFiles = dlg.FileNames;
				foreach (string fileName in selectedFiles)
				{
					GetOrAddFile(fileName);
				}
			}
		}

		private void AddDataSourceFromFolder()
		{
			var dlg = new VistaFolderBrowserDialog
			{
				Description = "Select folder to open",
				UseDescriptionForTitle = true
			};

			if (dlg.ShowDialog() == true)
			{
				var folder = dlg.SelectedPath;
				GetOrAddFolder(folder);
			}
		}

		private bool Represents(IDataSourceViewModel dataSourceViewModel, string fullName)
		{
			var file = dataSourceViewModel as FileDataSourceViewModel;
			if (file == null)
				return false;

			return string.Equals(file.FullName, fullName, StringComparison.InvariantCultureIgnoreCase);
		}

		private IDataSourceViewModel Add(IDataSource dataSource)
		{
			IDataSourceViewModel viewModel = CreateViewModel(dataSource);
			_observable.Add(viewModel);
			return viewModel;
		}

		private IDataSourceViewModel CreateViewModel(IDataSource dataSource)
		{
			if (dataSource == null)
				throw new ArgumentNullException(nameof(dataSource));

			IDataSourceViewModel viewModel;
			if (dataSource is IFileDataSource single)
			{
				viewModel = new FileDataSourceViewModel(single, _actionCenter);
			}
			else if (dataSource is IMergedDataSource merged)
			{
				viewModel = new MergedDataSourceViewModel(merged, _actionCenter);
			}
			else if (dataSource is IFolderDataSource folder)
			{
				viewModel = new FolderDataSourceViewModel(folder, _actionCenter);
			}
			else if (dataSource is ICustomDataSource custom)
			{
				viewModel = new CustomDataSourceViewModel(custom);
			}
			else
			{
				throw new ArgumentException(string.Format("Unknown data source: {0} ({1})", dataSource, dataSource.GetType()));
			}

			viewModel.Remove += OnRemove;
			_allDataSourceViewModels.Add(viewModel);

			if (_settings.DataSources.SelectedItem == viewModel.DataSource.Id)
			{
				SelectedItem = viewModel;
			}

			return viewModel;
		}

		private void OnRemove(IDataSourceViewModel viewModel)
		{
			int index = _observable.IndexOf(viewModel);
			if (index != -1)
			{
				_observable.RemoveAt(index);

				var merged = viewModel as MergedDataSourceViewModel;
				if (merged != null)
				{
					IEnumerable<IDataSourceViewModel> items = merged.Observable;
					foreach (IDataSourceViewModel item in items)
					{
						_observable.Insert(index++, item);
						item.Parent = null;
					}
				}
			}
			else if (viewModel.Parent != null)
			{
				((MergedDataSourceViewModel) viewModel.Parent).RemoveChild(viewModel);
				_dataSources.Remove(viewModel.DataSource);
			}

			_allDataSourceViewModels.Remove(viewModel);
			_dataSources.Remove(viewModel.DataSource);
			_settings.SaveAsync();
		}

		public bool CanBeDragged(IDataSourceViewModel source)
		{
			if (source is FolderDataSourceViewModel)
				return false; //< see https://github.com/Kittyfisto/Tailviewer/issues/125

			var sourceParent = source?.Parent;
			if (sourceParent is FolderDataSourceViewModel)
				return false; //< see https://github.com/Kittyfisto/Tailviewer/issues/125

			return true;
		}

		public bool CanBeDropped(IDataSourceViewModel source,
		                         IDataSourceViewModel dest,
		                         DataSourceDropType dropType,
		                         out IDataSourceViewModel finalDest)
		{
			finalDest = null;

			if (dropType == DataSourceDropType.None)
				return false;

			if (dest == null)
				return false;

			if (source is IMergedDataSourceViewModel)
			{
				if (dropType == DataSourceDropType.ArrangeBottom ||
				    dropType == DataSourceDropType.ArrangeTop)
				{
					// We cannot "rearrange" a group between parented data sources as that
					// would result in grouped groups - which is not supported (yet?).
					if (dest.Parent != null)
						return false;

					return true;
				}

				return false;
			}

			if (source == dest)
				return false;

			var single = dest as ISingleDataSourceViewModel;
			if (single != null)
				finalDest = single.Parent ?? dest;
			else
				finalDest = dest;

			if (finalDest is FolderDataSourceViewModel)
				return false; //< https://github.com/Kittyfisto/Tailviewer/issues/125

			var group = dest as IMergedDataSourceViewModel;
			if (group != null)
			{
				if (group.Observable.Count >= LogLineSourceId.MaxSources)
					return false;
			}

			return true;
		}

		public void OnDropped(IDataSourceViewModel source,
		                      IDataSourceViewModel dest,
		                      DataSourceDropType dropType)
		{
			if (!CanBeDragged(source))
				throw new ArgumentException("source");

			IDataSourceViewModel unused;
			if (!CanBeDropped(source, dest, dropType, out unused))
				throw new ArgumentException("source or dest");

			if (dropType == DataSourceDropType.Group)
			{
				DropToGroup(source, dest);
			}
			else
			{
				DropToArrange(source, dest, dropType);
			}
		}

		private void DropToArrange(IDataSourceViewModel source, IDataSourceViewModel dest, DataSourceDropType dropType)
		{
			IDataSourceViewModel sourceParent = source.Parent;
			if (sourceParent != null)
			{
				//
				// When the source is part of a group, then any arrange is going to remove it
				// from said group.
				//
				var group = ((MergedDataSourceViewModel) sourceParent);
				group.RemoveChild(source);
				if (sourceParent != dest.Parent)
				{
					//
					// If both source and dest are part of different groups, then
					// we need to check if source's group needs to be dissolved due to
					// having only 1 child left.
					//
					DissolveGroupIfNecessary(group);
				}
			}
			else
			{
				_observable.Remove(source);
			}

			if (dest.Parent != null)
			{
				//
				// If the destination has a parent then we need to insert
				// the source into it's collection.
				//
				var merged = ((MergedDataSourceViewModel) dest.Parent);
				int index = merged.Observable.IndexOf(dest);
				if (dropType.HasFlag(DataSourceDropType.ArrangeBottom))
					++index;

				merged.Insert(index, source);
			}
			else
			{
				//
				// Otherwise we insert the source into the flat, or root
				// collection.
				//
				int index = _observable.IndexOf(dest);
				if (dropType.HasFlag(DataSourceDropType.ArrangeBottom))
					++index;
				if (index < 0)
					index = 0;

				_observable.Insert(index, source);
				if (dropType.HasFlag(DataSourceDropType.ArrangeTop))
				{
					if (index + 1 < _observable.Count)
					{
						var tmp = source.DataSource.Settings;
						var before = _observable[index + 1].DataSource.Settings;
						_settings.DataSources.MoveBefore(tmp, before);
					}
				}
			}

			SelectedItem = source;
			_settings.SaveAsync();
		}

		private void DropToGroup(IDataSourceViewModel source, IDataSourceViewModel dest)
		{
			int sourceIndex = _observable.IndexOf(source);
			if (sourceIndex != -1)
			{
				DragFromUngrouped(source, dest, sourceIndex);
			}
			else
			{
				var parent = source.Parent as MergedDataSourceViewModel;
				if (parent != null)
				{
					DragFromGroup(source, dest, parent);
				}
			}
		}

		private void DragFromUngrouped(IDataSourceViewModel source, IDataSourceViewModel dest, int sourceIndex)
		{
			_observable.RemoveAt(sourceIndex);
			Drag(source, dest);
		}

		private void DragFromGroup(IDataSourceViewModel source, IDataSourceViewModel dest, MergedDataSourceViewModel parent)
		{
			parent.RemoveChild(source);
			DissolveGroupIfNecessary(parent);

			Drag(source, dest);
		}

		private void DissolveGroupIfNecessary(MergedDataSourceViewModel group)
		{
			if (group.ChildCount == 1)
			{
				int groupIndex = _observable.IndexOf(group);
				_observable.RemoveAt(groupIndex);
				_dataSources.Remove(group.DataSource);

				IDataSourceViewModel child = group.Observable.First();
				group.RemoveChild(child);
				_observable.Insert(groupIndex, child);
			}
		}

		private void Drag(IDataSourceViewModel source, IDataSourceViewModel dest)
		{
			var merged = dest as MergedDataSourceViewModel;
			if (merged != null)
			{
				// Drag from ungrouped onto an existing group
				// => add to existing group
				AddFileToGroup(source, merged);
				SelectedItem = merged;
			}
			else
			{
				// Drag from ungrouped onto a dest within a group
				// => find group of dest and addd to it
				merged = dest.Parent as MergedDataSourceViewModel;
				if (merged != null)
				{
					AddFileToGroup(source, merged);
					SelectedItem = merged;
				}
				else
				{
					// Drag from ungrouped onto another ungrouped source
					// => remove dest as well and create new group
					int destIndex = _observable.IndexOf(dest);
					if (destIndex != -1)
					{
						_observable.Remove(dest);

						MergedDataSource mergedDataSource = _dataSources.AddGroup();
						merged = new MergedDataSourceViewModel(mergedDataSource, _actionCenter);
						merged.Remove += OnRemove;
						merged.AddChild(source);
						merged.AddChild(dest);
						_observable.Insert(destIndex, merged);
						_allDataSourceViewModels.Add(merged);
						SelectedItem = merged;
					}
				}
			}
		}

		private static void AddFileToGroup(IDataSourceViewModel source, MergedDataSourceViewModel viewModel)
		{
			viewModel.AddChild(source);
		}

		internal IEnumerable<IDataSourceViewModel> AllDataSourceViewModels => _allDataSourceViewModels;
	}
}