using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MergedDataSourceViewModel
		: AbstractDataSourceViewModel
	{
		private readonly ObservableCollection<IDataSourceViewModel> _observable;
		private readonly MergedDataSource _dataSource;

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

		public void RemoveChild(IDataSourceViewModel dataSource)
		{
			if (dataSource.Parent != this)
				throw new ArgumentException("dataSource.Parent");

			_observable.Remove(dataSource);
			_dataSource.Remove(dataSource.DataSource);
			dataSource.Parent = null;
		}

		public override ICommand OpenInExplorerCommand
		{
			get { return null; }
		}

		public override string DisplayName
		{
			get { return "Merged Data Source"; }
		}

		public int ChildCount
		{
			get { return _observable.Count; }
		}
	}
}