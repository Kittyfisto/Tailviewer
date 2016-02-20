using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MergedDataSourceViewModel
		: AbstractDataSourceViewModel
	{
		private readonly ObservableCollection<IDataSourceViewModel> _observable;
		private readonly MergedDataSource _dataSource;
		private bool _displayNoTimestampCount;
		private int _noTimestampSum;

		public MergedDataSourceViewModel(MergedDataSource dataSource)
			: base(dataSource)
		{
			_dataSource = dataSource;
			_observable = new ObservableCollection<IDataSourceViewModel>();
		}

		public override string ToString()
		{
			return DisplayName;
		}

		public IEnumerable<IDataSourceViewModel> Observable
		{
			get { return _observable; }
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

		public override ICommand OpenInExplorerCommand
		{
			get { return null; }
		}

		public override string DisplayName
		{
			get { return "Merged Data Source"; }
		}

		public override void Update()
		{
			base.Update();

			if (_observable != null)
				NoTimestampSum = _observable.Sum(x => x.NoTimestampCount);
		}

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

		public int ChildCount
		{
			get { return _observable.Count; }
		}
	}
}