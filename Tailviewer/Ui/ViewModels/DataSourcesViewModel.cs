using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class DataSourcesViewModel
	{
		private readonly DataSources _dataSources;
		private readonly ObservableCollection<IDataSourceViewModel> _observable;
		private readonly ApplicationSettings _settings;

		public DataSourcesViewModel(ApplicationSettings settings, DataSources dataSources)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (dataSources == null) throw new ArgumentNullException("dataSources");

			_settings = settings;
			_observable = new ObservableCollection<IDataSourceViewModel>();
			_dataSources = dataSources;
			foreach (IDataSource dataSource in dataSources)
			{
				Add(dataSource);
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
				IDataSource dataSource = _dataSources.Add(fileName);
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
			var viewModel = new SingleDataSourceViewModel((SingleDataSource) dataSource);
			viewModel.Remove += OnRemove;
			_observable.Add(viewModel);
			return viewModel;
		}

		private void OnRemove(IDataSourceViewModel viewModel)
		{
			int index = _observable.IndexOf(viewModel);
			if (index != -1)
			{
				_observable.RemoveAt(index);
				_dataSources.Remove(viewModel.DataSource);

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

				_settings.Save();
			}
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

			switch (dropType)
			{
				case DataSourceDropType.Group:
					DropToGroup(source, dest);
					break;

				case DataSourceDropType.ArrangeBottom:
				case DataSourceDropType.ArrangeTop:
					DropToArrange(source, dest, dropType);
					break;

				default:
					throw new ArgumentOutOfRangeException("dropType");
			}
		}

		private void DropToArrange(IDataSourceViewModel source, IDataSourceViewModel dest, DataSourceDropType dropType)
		{
			if (source.Parent != null)
			{
				var group = ((MergedDataSourceViewModel) source.Parent);
				group.RemoveChild(source);
				DissolveGroupIfNecessary(group);
			}
			else
			{
				_observable.Remove(source);
			}

			if (dest.Parent != null)
			{
				var merged = ((MergedDataSourceViewModel) dest.Parent);
				int index = merged.Observable.IndexOf(source);
				if (dropType == DataSourceDropType.ArrangeBottom)
					++index;

				merged.Insert(index, source);
			}
			else
			{
				int index = _observable.IndexOf(dest);
				if (dropType == DataSourceDropType.ArrangeBottom)
					++index;

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

			var parent = source.Parent as MergedDataSourceViewModel;
			if (parent != null)
			{
				DragFromGroup(source, dest, parent);
			}
		}

		private void DragFromUngrouped(IDataSourceViewModel source, IDataSourceViewModel dest, int sourceIndex)
		{
			_observable.RemoveAt(sourceIndex);
			Drag(source, dest, sourceIndex);
		}

		private void DragFromGroup(IDataSourceViewModel source, IDataSourceViewModel dest, MergedDataSourceViewModel parent)
		{
			parent.RemoveChild(source);
			DissolveGroupIfNecessary(parent);

			Drag(source, dest, -1);
		}

		private void DissolveGroupIfNecessary(MergedDataSourceViewModel group)
		{
			if (group.ChildCount == 1)
			{
				int groupIndex = _observable.IndexOf(group);
				_observable.RemoveAt(groupIndex);
				IDataSourceViewModel child = group.Observable.First();
				child.Parent = null;
				_observable.Insert(groupIndex, child);
			}
		}

		private void Drag(IDataSourceViewModel source, IDataSourceViewModel dest, int sourceIndex)
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

						var mergedDataSource = (MergedDataSource) _dataSources.Add(new DataSource());
						merged = new MergedDataSourceViewModel(mergedDataSource);
						merged.Remove += OnRemove;
						merged.AddChild(source);
						merged.AddChild(dest);
						_observable.Insert(destIndex, merged);
						merged.IsOpen = true;
					}
				}
			}
			return;
		}

		private static void AddFileToGroup(IDataSourceViewModel source, MergedDataSourceViewModel viewModel)
		{
			viewModel.AddChild(source);
		}
	}
}