using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.FileExplorer;
using Tailviewer.Settings;
using Tailviewer.Ui.ContextMenu;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Ui.DataSourceTree
{
	/// <summary>
	///     Represents a data source and is capable of opening the source folder in explorer
	/// </summary>
	public sealed class FileDataSourceViewModel
		: AbstractDataSourceViewModel
			, IFileDataSourceViewModel
	{
		private readonly IActionCenter _actionCenter;
		private readonly IFileDataSource _dataSource;
		private readonly string _fileName;
		private bool _canBeRemoved;
		private bool _displayNoTimestampCount;
		private bool _excludeFromParent;
		private string _folder;

		public FileDataSourceViewModel(IFileDataSource dataSource,
		                               IActionCenter actionCenter,
		                               IApplicationSettings applicationSettings)
			: base(dataSource, actionCenter, applicationSettings)
		{
			_actionCenter = actionCenter ?? throw new ArgumentNullException(nameof(actionCenter));
			_dataSource = dataSource;
			_fileName = Path.GetFileName(dataSource.FullFileName);
			_canBeRemoved = true;

			Update();
			UpdateFolder();

			AddFileMenuItems(new []
			{
				new CommandMenuViewModel(new DelegateCommand2(OpenInExplorer))
				{
					Header = "Open Containing Folder",
					ToolTip = "Opens Folder the current Data Source is contained in"
				}
			});

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

		public string Folder
		{
			get { return _folder; }
		}

		public string FullName
		{
			get { return _dataSource.FullFileName; }
		}

		public override string DisplayName
		{
			get { return _fileName; }
			set { throw new InvalidOperationException(); }
		}

		public override bool CanBeRenamed
		{
			get { return false; }
		}

		public override string DataSourceOrigin
		{
			get { return FullName; }
		}

		public bool CanBeRemoved
		{
			get { return _canBeRemoved; }
			private set
			{
				if (value == _canBeRemoved)
					return;

				_canBeRemoved = value;
				EmitPropertyChanged();
			}
		}

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

		public bool ExcludeFromParent
		{
			get { return _excludeFromParent; }
			set
			{
				if (value == _excludeFromParent)
					return;

				_excludeFromParent = value;
				if (Parent?.DataSource is IMultiDataSource parentDataSource)
					parentDataSource.SetExcluded(_dataSource, _excludeFromParent);

				foreach (var item in ContextMenuItems.OfType<ToggleExcludeFromGroupViewModel>())
					item.UpdateHeader();

				EmitPropertyChanged();
			}
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Parent):
					if (Parent?.DataSource is IMultiDataSource parentDataSource &&
					    parentDataSource.IsExcluded(_dataSource)) ExcludeFromParent = true;
					break;

				case nameof(IsGrouped):
					UpdateDisplayNoTimestampCount();
					UpdateFolder();
					UpdateCanBeRemoved();
					break;

				case nameof(NoTimestampCount):
					UpdateDisplayNoTimestampCount();
					break;
			}
		}

		private void UpdateFolder()
		{
			var folderViewModel = Parent as FolderDataSourceViewModel;
			string path;
			if (folderViewModel != null)
			{
				var folderRoot = folderViewModel.DataSourceOrigin;
				path = "<root>\\" + Path.GetDirectoryName(GetRelativePath(folderRoot, _dataSource.FullFileName));
			}
			else
			{
				path = Path.GetDirectoryName(_dataSource.FullFileName);
			}

			if (path == null || !path.EndsWith("\\"))
				path += "\\";

			_folder = path;
			EmitPropertyChanged(nameof(Folder));
		}

		private void UpdateCanBeRemoved()
		{
			CanBeRemoved = !(Parent is FolderDataSourceViewModel);

			if (Parent?.DataSource is IMultiDataSource parentDataSource)
				SetContextMenuItems(new[] {new ToggleExcludeFromGroupViewModel(this)});
			else
				SetContextMenuItems(Enumerable.Empty<IMenuViewModel>());
		}

		[Pure]
		private static string GetRelativePath(string folderRoot, string fullFilePath)
		{
			if (string.IsNullOrEmpty(folderRoot))
				return fullFilePath;

			if (!folderRoot.EndsWith("\\") && !folderRoot.EndsWith("/"))
				folderRoot += '\\';

			var fullPath = new Uri(fullFilePath);
			var folder = new Uri(folderRoot);
			var relativePath =
				Uri.UnescapeDataString(
				                       folder.MakeRelativeUri(fullPath)
				                             .ToString()
				                             .Replace(oldChar: '/', Path.DirectorySeparatorChar)
				                      );
			return relativePath;
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