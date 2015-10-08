using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using SharpTail.BusinessLogic;

namespace SharpTail.Ui.ViewModels
{
	public sealed class LogViewerViewModel
		: INotifyPropertyChanged
		  , ILogFileListener
	{
		private readonly IDispatcher _dispatcher;
		private readonly LogFile _fullLogFile;
		private readonly List<KeyValuePair<ILogFile, LogFileSection>> _pendingSections;
		private readonly ObservableCollection<LogEntryViewModel> _logEntries;
		private int _allLogEntryCount;
		private ILogFile _currentLogFile;
		private string _filterString;
		private int _logEntryCount;

		public LogViewerViewModel(IDispatcher dispatcher, LogFile logFile)
		{
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");
			if (logFile == null) throw new ArgumentNullException("logFile");

			_dispatcher = dispatcher;
			_fullLogFile = logFile;
			_fullLogFile.Start();

			_pendingSections = new List<KeyValuePair<ILogFile, LogFileSection>>();

			_logEntries = new ObservableCollection<LogEntryViewModel>();

			SetCurrentLogFile(_fullLogFile);
		}

		public ILogFile CurrentLogFile
		{
			get { return _currentLogFile; }
		}

		private void SetCurrentLogFile(ILogFile file)
		{
			if (file == null) throw new ArgumentNullException("file");

			ClearAllEntries();

			if (_currentLogFile != null)
			{
				_currentLogFile.Remove(this);
				if (_currentLogFile != _fullLogFile)
				{
					_currentLogFile.Dispose();
				}
				_currentLogFile = null;
			}

			_currentLogFile = file;
			file.AddListener(this, TimeSpan.FromMilliseconds(10), 1000);
			UpdateCounts();
		}

		private void ClearAllEntries()
		{
			_logEntries.Clear();
			AllLogEntryCount = 0;
		}

		public string FilterString
		{
			get { return _filterString; }
			set
			{
				if (value == _filterString)
					return;

				_filterString = value;

				var file = string.IsNullOrEmpty(value)
					           ? (ILogFile)_fullLogFile
					           : (ILogFile)_fullLogFile.Filter(value);

				SetCurrentLogFile(file);

				EmitPropertyChanged();
			}
		}

		public int LogEntryCount
		{
			get { return _logEntryCount; }
			private set
			{
				if (value == _logEntryCount)
					return;

				_logEntryCount = value;
				EmitPropertyChanged();
			}
		}

		public int AllLogEntryCount
		{
			get { return _allLogEntryCount; }
			private set
			{
				if (value == _allLogEntryCount)
					return;

				_allLogEntryCount = value;
				EmitPropertyChanged();
			}
		}

		public ObservableCollection<LogEntryViewModel> LogEntries
		{
			get { return _logEntries; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void OnLogFileModified(LogFileSection section)
		{
			lock (_pendingSections)
			{
				_pendingSections.Add(new KeyValuePair<ILogFile, LogFileSection>(_currentLogFile, section));
				_dispatcher.BeginInvoke(Synchronize, DispatcherPriority.Background);
			}
		}

		private void Synchronize()
		{
			lock (_pendingSections)
			{
				foreach (var pair in _pendingSections)
				{
					var file = pair.Key;
					if (file != _currentLogFile)
						continue; //< This message belongs to an old change and must be ignored

					var section = pair.Value;
					var entries = _currentLogFile.GetSection(section);
					for(int i = 0; i < entries.Length; ++i)
					{
						var model = new LogEntryViewModel(section.Index + i, entries[i], _filterString);
						_logEntries.Add(model);
					}
				}

				_pendingSections.Clear();
			}

			UpdateCounts();
		}

		private void UpdateCounts()
		{
			LogEntryCount = _currentLogFile.Count;
			AllLogEntryCount = _fullLogFile.Count;
		}
	}
}