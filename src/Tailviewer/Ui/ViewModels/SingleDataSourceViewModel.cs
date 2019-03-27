using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.FileExplorer;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     Represents a data source and is capable of opening the source folder in explorer
	/// </summary>
	public sealed class SingleDataSourceViewModel
		: AbstractDataSourceViewModel
		, ISingleDataSourceViewModel
	{
		private readonly IActionCenter _actionCenter;
		private readonly ISingleDataSource _dataSource;
		private readonly string _fileName;
		private readonly string _folder;
		private readonly ICommand _openInExplorerCommand;
		private bool _displayNoTimestampCount;

		public SingleDataSourceViewModel(ISingleDataSource dataSource,
							IActionCenter actionCenter)
								: base(dataSource)
		{
			if (actionCenter == null) throw new ArgumentNullException(nameof(actionCenter));

			_actionCenter = actionCenter;
			_dataSource = dataSource;
			_fileName = Path.GetFileName(dataSource.FullFileName);
			_folder = Path.GetDirectoryName(dataSource.FullFileName);
			_openInExplorerCommand = new DelegateCommand(OpenInExplorer);
			Update();

			UpdateDisplayNoTimestampCount();
			PropertyChanged += OnPropertyChanged;
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

		public override ICommand OpenInExplorerCommand => _openInExplorerCommand;

		public override string DisplayName
		{
			get { return _fileName; }
			set { throw new InvalidOperationException(); }
		} 

		public override bool CanBeRenamed => false;

		public override string DataSourceOrigin => FullName;

		public string FileName => _fileName;

		public string Folder => _folder;

		public string FullName => _dataSource.FullFileName;

		public string CharacterCode
		{
			get { return _dataSource.CharacterCode; }
			set
			{
				if (value == _dataSource.CharacterCode)
					return;

				_dataSource.CharacterCode = value;
				EmitPropertyChanged();
			}
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsGrouped":
				case "NoTimestampCount":
					UpdateDisplayNoTimestampCount();
					break;
			}
		}

		private void UpdateDisplayNoTimestampCount()
		{
			DisplayNoTimestampCount = IsGrouped && NoTimestampCount > 0;
		}

		public override string ToString()
		{
			return DisplayName;
		}

		private void OpenInExplorer()
		{
			var action = new OpenFolderAction(FullName, new FileExplorer());
			_actionCenter.Add(action);
		}

	}
}