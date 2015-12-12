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
		private readonly DataSourceViewModel _dataSource;
		private int _allLogEntryCount;
		private ILogFile _currentLogFile;
		private int _logEntryCount;
		private Size _fileSize;

		public LogViewerViewModel(DataSourceViewModel dataSource, IDispatcher dispatcher, LogFile logFile)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");
			if (logFile == null) throw new ArgumentNullException("logFile");

			_dataSource = dataSource;
			_dispatcher = dispatcher;
			_fullLogFile = logFile;
			_fullLogFile.Start();

			_pendingSections = new List<KeyValuePair<ILogFile, LogFileSection>>();
			_logEntries = new ObservableCollection<LogEntryViewModel>();

			UpdateCurrentLogFile();
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

		public string StringFilter
		{
			get { return _dataSource.StringFilter; }
			set
			{
				if (value == StringFilter)
					return;

				_dataSource.StringFilter = value;
				UpdateCurrentLogFile();
				EmitPropertyChanged();
			}
		}

		private void UpdateCurrentLogFile()
		{
			var stringFilter = StringFilter;
			var levels = LevelsFilter;
			ILogFile logFile;
			if (!string.IsNullOrEmpty(stringFilter) || levels != LevelFlags.All)
			{
				logFile = _fullLogFile.Filter(stringFilter, levels);
			}
			else
			{
				logFile = _fullLogFile;
			}

			SetCurrentLogFile(logFile);
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

		public Size FileSize
		{
			get { return _fileSize; }
			private set
			{
				if (value == _fileSize)
					return;

				_fileSize = value;
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

		public bool FollowTail
		{
			get { return _dataSource.FollowTail; }
			set
			{
				if (value == FollowTail)
					return;

				_dataSource.FollowTail = value;
				EmitPropertyChanged();
			}
		}

		public LevelFlags LevelsFilter
		{
			get { return _dataSource.LevelsFilter; }
			set
			{
				if (value == LevelsFilter)
					return;

				_dataSource.LevelsFilter = value;
				UpdateCurrentLogFile();
				EmitPropertyChanged();
			}
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
						var model = new LogEntryViewModel(section.Index + i, entries[i]);
						_logEntries.Add(model);
					}
				}

				_pendingSections.Clear();
				_dataSource.UpdateLastWritten();
			}

			UpdateCounts();
		}

		private void UpdateCounts()
		{
			LogEntryCount = _currentLogFile.Count;
			AllLogEntryCount = _fullLogFile.Count;
			FileSize = _fullLogFile.FileSize;
		}
	}
}