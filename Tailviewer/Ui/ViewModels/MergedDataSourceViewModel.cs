using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using log4net;
using Metrolib;
using Metrolib.Controls;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class MergedDataSourceViewModel
		: AbstractDataSourceViewModel
		, ITreeViewItemViewModel
		, IMergedDataSourceViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IMergedDataSource _dataSource;
		private readonly ObservableCollection<IDataSourceViewModel> _observable;
		private readonly DelegateCommand _openInExplorerCommand;
		private bool _displayNoTimestampCount;
		private int _noTimestampSum;
		private bool _isSelected;

		public MergedDataSourceViewModel(MergedDataSource dataSource)
			: base(dataSource)
		{
			_dataSource = dataSource;
			_observable = new ObservableCollection<IDataSourceViewModel>();
			_openInExplorerCommand = new DelegateCommand(OpenInExplorer);
		}

		private void OpenInExplorer()
		{
			var dataSources = _dataSource.OriginalSources.GroupBy(GetDirectory);
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

		public IReadOnlyList<IDataSourceViewModel> Observable => _observable;

		public override ICommand OpenInExplorerCommand => _openInExplorerCommand;

		public override string DisplayName
		{
			get {return _dataSource.DisplayName; }
			set
			{
				if (value == _dataSource.DisplayName)
					return;

				_dataSource.DisplayName = value;
				EmitPropertyChanged();
				EmitPropertyChanged(nameof(DataSourceOrigin));
			}
		}

		public override bool CanBeRenamed => true;

		public override string DataSourceOrigin => DisplayName;

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

		public bool AddChild(IDataSourceViewModel dataSource)
		{
			if (dataSource.Parent != null)
				throw new ArgumentException("dataSource.Parent");

			if (_observable.Count >= LogLineSourceId.MaxSources)
			{
				Log.InfoFormat("Cannot add source '{0}': The maximum number of sources in this group has been reached", dataSource);
				return false;
			}

			_observable.Add(dataSource);
			_dataSource.Add(dataSource.DataSource);
			dataSource.Parent = this;

			DistributeCharacterCodes();
			return true;
		}

		public void Insert(int index, IDataSourceViewModel dataSource)
		{
			if (dataSource.Parent != null)
				throw new ArgumentException("dataSource.Parent");

			_observable.Insert(index, dataSource);
			_dataSource.Add(dataSource.DataSource);
			dataSource.Parent = this;
			Update();

			DistributeCharacterCodes();
		}

		public void RemoveChild(IDataSourceViewModel dataSource)
		{
			if (dataSource.Parent != this)
				throw new ArgumentException("dataSource.Parent");

			_observable.Remove(dataSource);
			_dataSource.Remove(dataSource.DataSource);
			dataSource.Parent = null;
			Update();

			DistributeCharacterCodes();
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

		public DataSourceDisplayMode DisplayMode
		{
			get { return _dataSource.DisplayMode; }
			set
			{
				if (value == _dataSource.DisplayMode)
					return;

				_dataSource.DisplayMode = value;
				EmitPropertyChanged();
			}
		}

		private void DistributeCharacterCodes()
		{
			for (int i = 0; i < _observable.Count; ++i)
			{
				var viewModel = _observable[i] as ISingleDataSourceViewModel;
				if (viewModel != null)
				{
					var characterCode = GenerateCharacterCode(i);
					viewModel.CharacterCode = characterCode;
				}
			}
		}

		/// <summary>
		///     Generates the character code for the n-th data source.
		/// </summary>
		/// <param name="dataSourceIndex"></param>
		/// <returns></returns>
		private static string GenerateCharacterCode(int dataSourceIndex)
		{
			var builder = new StringBuilder(capacity: 2);
			do
			{
				var value = (char)('A' + dataSourceIndex % 26);
				builder.Append(value);
				dataSourceIndex /= 26;
			} while (dataSourceIndex > 0);
			return builder.ToString();
		}
	}
}