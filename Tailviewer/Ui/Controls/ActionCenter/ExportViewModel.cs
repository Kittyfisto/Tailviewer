using System.Diagnostics;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Core;

namespace Tailviewer.Ui.Controls.ActionCenter
{
	public sealed class ExportViewModel
		: AbstractNotificationViewModel
	{
		private readonly IExportAction _export;
		private readonly DelegateCommand _openInExplorerCommand;
		private bool _isFinished;
		private Percentage _progress;
		private bool _isSuccessful;
		private string _fullExportFilename;
		private string _errorMessage;

		public ExportViewModel(IExportAction export)
			: base(export)
		{
			_export = export;
			_openInExplorerCommand = new DelegateCommand(OpenInExplorer, CanOpenInExplorer);
		}

		public string DataSourceName => _export.DataSourceName;

		public string Destination => _export.Destination;

		public Percentage Progress
		{
			get { return _progress; }
			private set
			{
				if (value == _progress)
					return;

				_progress = value;
				EmitPropertyChanged();

				if (_progress >= Percentage.HundredPercent)
					IsFinished = true;
			}
		}

		public bool IsFinished
		{
			get { return _isFinished; }
			private set
			{
				if (_isFinished == value)
					return;

				_isFinished = value;
				EmitPropertyChanged();
			}
		}

		public bool IsSuccessful
		{
			get { return _isSuccessful; }
			private set
			{
				if (value == _isSuccessful)
					return;

				_isSuccessful = value;
				EmitPropertyChanged();
			}
		}

		public ICommand OpenInExplorerCommand => _openInExplorerCommand;

		private bool CanOpenInExplorer()
		{
			return true;
		}

		private void OpenInExplorer()
		{
			string argument = string.Format(@"/select, {0}", _export.FullExportFilename);
			Process.Start("explorer.exe", argument);
		}

		public override void Update()
		{
			Progress = _export.Progress;
			if (IsFinished)
			{
				FullExportFilename = _export.FullExportFilename;
				IsSuccessful = _export.Exception == null;
				ErrorMessage = _export.Exception?.Message;
			}
		}

		public string FullExportFilename
		{
			get { return _fullExportFilename; }
			private set
			{
				if (value == _fullExportFilename)
					return;

				_fullExportFilename = value;
				EmitPropertyChanged();
			}
		}

		public string ErrorMessage
		{
			get { return _errorMessage; }
			set
			{
				if (value == _errorMessage)
					return;

				_errorMessage = value;
				EmitPropertyChanged();
			}
		}
	}
}