using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class DataSourcesViewModel
		: INotifyPropertyChanged
	{
		private readonly DataSources _dataSources;
		private readonly ObservableCollection<IDataSourceViewModel> _observable;
		private readonly ApplicationSettings _settings;
		private IDataSourceViewModel _selectedItem;

		public DataSourcesViewModel(ApplicationSettings settings, DataSources dataSources)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (dataSources == null) throw new ArgumentNullException("dataSources");

			_settings = settings;
			_observable = new ObservableCollection<IDataSourceViewModel>();
			_dataSources = dataSources;
			foreach (IDataSource dataSource in dataSources)
			{
				if (dataSource.ParentId == Guid.Empty)
				{
					Add(dataSource);
				}
			}

			foreach (IDataSource dataSource in dataSources)
			{
				Guid parentId = dataSource.ParentId;
				if (parentId != Guid.Empty)
				{
					IDataSourceViewModel parent = _observable.First(x => x.DataSource.Id == parentId);
					var group = (MergedDataSourceViewModel) parent;
					IDataSourceViewModel viewModel = CreateViewModel(dataSource);
					group.AddChild(viewModel);
				}
			}
		}

		public IDataSourceViewModel SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				if (value == _selectedItem)
					return;

				_selectedItem = value;
				_settings.DataSources.SelectedItem = value != null ? value.DataSource.Id : Guid.Empty;
				EmitPropertyChanged();
			}
		}

		public ObservableCollection<IDataSourceViewModel> Observable
		{
			get { return _observable; }
		}

		public void Update()
		{
			foreach (IDataSourceViewModel dataSource in _observable)
			{
				dataSource.Update();
			}
		}

		public IDataSourceViewModel GetOrAdd(string fileName)
		{
			string fullName = Path.GetFullPath(fileName);
			IDataSourceViewModel viewModel =
				_observable.FirstOrDefault(x => Represents(x, fullName));
			if (viewModel == null)
			{
				IDataSource dataSource = _dataSources.AddDataSource(fileName);
				viewModel = Add(dataSource);
				_settings.Save();
			}

			return viewModel;
		}

		private bool Represents(IDataSourceViewModel dataSourceViewModel, string fullName)
		{
			var file = dataSourceViewModel as SingleDataSourceViewModel;
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
				throw new ArgumentNullException("dataSource");

			IDataSourceViewModel viewModel;
			var single = dataSource as SingleDataSource;
			if (single != null)
			{
				viewModel = new SingleDataSourceViewModel(single);
			}
			else
			{
				var merged = dataSource as MergedDataSource;
				if (merged != null)
				{
					viewModel = new MergedDataSourceViewModel(merged);
				}
				else
				{
					throw new ArgumentException(string.Format("Unknown data source: {0} ({1})", dataSource, dataSource.GetType()));
				}
			}
			viewModel.Remove += OnRemove;

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

			_dataSources.Remove(viewModel.DataSource);
			_settings.Save();
		}

		public bool CanBeDragged(IDataSourceViewModel source)
		{
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

			if (source is MergedDataSourceViewModel)
			{
				if (dropType == DataSourceDropType.ArrangeBottom ||
				    dropType == DataSourceDropType.ArrangeTop)
					return true;

				return false;
			}

			if (source == dest)
				return false;

			var single = dest as SingleDataSourceViewModel;
			if (single != null)
				finalDest = single.Parent ?? dest;
			else
				finalDest = dest;

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
			}
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
			}
			else
			{
				// Drag from ungrouped onto a dest within a group
				// => find group of dest and addd to it
				merged = dest.Parent as MergedDataSourceViewModel;
				if (merged != null)
				{
					AddFileToGroup(source, merged);
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
						merged = new MergedDataSourceViewModel(mergedDataSource);
						merged.Remove += OnRemove;
						merged.AddChild(source);
						merged.AddChild(dest);
						_observable.Insert(destIndex, merged);
						SelectedItem = merged;
					}
				}
			}
		}

		private static void AddFileToGroup(IDataSourceViewModel source, MergedDataSourceViewModel viewModel)
		{
			viewModel.AddChild(source);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}