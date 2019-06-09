﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class FolderDataSourceViewModel
		: AbstractDataSourceViewModel
		, IMergedDataSourceViewModel
	{
		private readonly IFolderDataSource _dataSource;
		private readonly IActionCenter _actionCenter;
		private readonly Dictionary<IDataSource, IDataSourceViewModel> _dataSourceViewModelsByDataSource;
		private readonly ObservableCollection<IDataSourceViewModel> _dataSourceViewModels;
		private readonly string _displayName;
		private string _fileReport;

		public FolderDataSourceViewModel(IFolderDataSource folder, IActionCenter actionCenter)
			: base(folder)
		{
			_dataSource = folder;
			var path = folder.LogFileFolderPath;
			if (!string.IsNullOrEmpty(path))
				_displayName = Path.GetFileName(path);

			_actionCenter = actionCenter;
			_dataSourceViewModelsByDataSource = new Dictionary<IDataSource, IDataSourceViewModel>();
			_dataSourceViewModels = new ObservableCollection<IDataSourceViewModel>();

			UpdateFileReport();
		}

		#region Overrides of AbstractDataSourceViewModel

		public override ICommand OpenInExplorerCommand => null;

		public override string DisplayName
		{
			get => _displayName;
			set { throw new NotImplementedException(); }
		}

		public override bool CanBeRenamed => false;

		public override string DataSourceOrigin => _dataSource.LogFileFolderPath;

		public string FileReport
		{
			get { return _fileReport; }
			private set
			{
				if (value == _fileReport)
					return;

				_fileReport = value;
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

		public IReadOnlyList<IDataSourceViewModel> Observable => _dataSourceViewModels;

		public override void Update()
		{
			base.Update();

			var dataSources = _dataSource.OriginalSources;
			bool changed = AddNewDataSources(dataSources);
			changed |= RemoveOldDataSources(dataSources);

			if (changed)
			{
				DistributeCharacterCodes();
			}

			if (changed || FileReport == null)
				UpdateFileReport();

			UpdateDataSources();
		}

		private bool AddNewDataSources(IReadOnlyList<IDataSource> dataSources)
		{
			bool changed = false;
			foreach (var dataSource in dataSources)
			{
				if (!_dataSourceViewModelsByDataSource.TryGetValue(dataSource, out var viewModel))
				{
					viewModel = new SingleDataSourceViewModel((ISingleDataSource) dataSource, _actionCenter)
					{
						Parent = this
					};
					_dataSourceViewModelsByDataSource.Add(dataSource, viewModel);
					_dataSourceViewModels.Add(viewModel);
					changed = true;
				}
			}

			return changed;
		}

		private bool RemoveOldDataSources(IReadOnlyList<IDataSource> dataSources)
		{
			var toRemove = new List<IDataSource>();
			foreach (var dataSource in _dataSourceViewModelsByDataSource.Keys)
			{
				if (!dataSources.Contains(dataSource))
				{
					toRemove.Add(dataSource);
				}
			}

			foreach (var dataSource in toRemove)
			{
				if (_dataSourceViewModelsByDataSource.TryGetValue(dataSource, out var viewModel))
				{
					_dataSourceViewModelsByDataSource.Remove(dataSource);
					_dataSourceViewModels.Remove(viewModel);
				}
			}

			return toRemove.Count > 0;
		}

		private void UpdateFileReport()
		{
			var unfilteredCount = _dataSource.UnfilteredFileCount;
			var filteredCount = _dataSource.FilteredFileCount;
			var actualCount = _dataSource.OriginalSources.Count;

			var reportBuilder = new StringBuilder();

			if (unfilteredCount == filteredCount)
			{
				reportBuilder.AppendFormat("Monitoring {0}", Language.Count("file", actualCount));
			}
			else
			{
				var notMatchingCount = unfilteredCount - filteredCount;
				reportBuilder.AppendFormat("Monitoring {0} ({1} not matching filter)",
				                           Language.Count("file", actualCount),
				                           notMatchingCount);
			}

			var skipped = filteredCount - actualCount;
			if (skipped > 0)
			{
				reportBuilder.AppendLine();
				reportBuilder.AppendFormat("Skipping {0} (due to internal limitation)",
				                           Language.Count("file", skipped));
			}

			FileReport = reportBuilder.ToString();
		}

		private void UpdateDataSources()
		{
			foreach (var dataSource in _dataSourceViewModels)
			{
				dataSource.Update();
			}
		}

		#endregion

		private void DistributeCharacterCodes()
		{
			for (int i = 0; i < _dataSourceViewModels.Count; ++i)
			{
				var viewModel = _dataSourceViewModels[i] as ISingleDataSourceViewModel;
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