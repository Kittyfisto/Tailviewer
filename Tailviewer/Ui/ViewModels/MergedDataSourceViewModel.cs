using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Metrolib;
using Metrolib.Controls;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MergedDataSourceViewModel
		: AbstractDataSourceViewModel
		, ITreeViewItemViewModel
	{
		private readonly MergedDataSource _dataSource;
		private readonly ObservableCollection<IDataSourceViewModel> _observable;
		private bool _displayNoTimestampCount;
		private int _noTimestampSum;
		private bool _isSelected;
		private DelegateCommand _openInExplorerCommand;

		public MergedDataSourceViewModel(MergedDataSource dataSource)
			: base(dataSource)
		{
			_dataSource = dataSource;
			_observable = new ObservableCollection<IDataSourceViewModel>();
			_openInExplorerCommand = new DelegateCommand(OpenInExplorer);
		}

		private void OpenInExplorer()
		{
			var dataSources = _dataSource.DataSources.GroupBy(GetDirectory);
			foreach (var grouping in dataSources)
			{
				SingleDataSourceViewModel.OpenInExplorer(grouping.First());
			}
		}

		private string GetDirectory(IDataSource arg)
		{
			var fname = arg.FullFileName;
			var dir = Path.GetDirectoryName(fname);
			return dir;
		}

		public IEnumerable<IDataSourceViewModel> Observable => _observable;

		public override ICommand OpenInExplorerCommand => _openInExplorerCommand;

		public override string DisplayName => "Merged Data Source";

		public int NoTimestampSum
		{
			get { return _noTimestampSum; }
			private set
			{
				if (value == _noTimestampSum)
					return;

				_noTimestampSum = value;
				EmitPropertyChanged();

				DisplayNoTimestampCount = value > 0;
			}
		}

		public bool DisplayNoTimestampCount
		{
			get { return _displayNoTimestampCount; }
			private set
			{
				if (value == _displayNoTimestampCount)
					return;

				_displayNoTimestampCount = value;
				EmitPropertyChanged();
			}
		}

		public int ChildCount => _observable.Count;

		public override string ToString()
		{
			return DisplayName;
		}

		public void AddChild(IDataSourceViewModel dataSource)
		{
			if (dataSource.Parent != null)
				throw new ArgumentException("dataSource.Parent");

			_observable.Add(dataSource);
			_dataSource.Add(dataSource.DataSource);
			dataSource.Parent = this;
		}

		public void Insert(int index, IDataSourceViewModel dataSource)
		{
			if (dataSource.Parent != null)
				throw new ArgumentException("dataSource.Parent");

			_observable.Insert(index, dataSource);
			_dataSource.Add(dataSource.DataSource);
			dataSource.Parent = this;
			Update();
		}

		public void RemoveChild(IDataSourceViewModel dataSource)
		{
			if (dataSource.Parent != this)
				throw new ArgumentException("dataSource.Parent");

			_observable.Remove(dataSource);
			_dataSource.Remove(dataSource.DataSource);
			dataSource.Parent = null;
			Update();
		}

		public override void Update()
		{
			base.Update();

			if (_observable != null)
				NoTimestampSum = _observable.Sum(x => x.NoTimestampCount);
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected)
					return;

				_isSelected = value;
				EmitPropertyChanged();
			}
		}

		public bool IsExpanded
		{
			get { return _dataSource.IsExpanded; }
			set
			{
				if (value == _dataSource.IsExpanded)
					return;

				_dataSource.IsExpanded = value;
				EmitPropertyChanged();
			}
		}
	}
}