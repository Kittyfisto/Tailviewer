using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class LogViewerViewModel
		: INotifyPropertyChanged
		  , ILogFileListener
		, IDisposable
	{
		private readonly IDispatcher _dispatcher;
		private readonly LogFile _fullLogFile;
		private readonly List<KeyValuePair<ILogFile, LogFileSection>> _pendingSections;
		private readonly ObservableCollection<LogEntryViewModel> _logEntries;
		private readonly DataSourceViewModel _dataSource;
		private ILogFile _currentLogFile;
		private int _logEntryCount;
		private IEnumerable<ILogEntryFilter> _quickFilterChain;
		private readonly TimeSpan _maximumWaitTime;

		public LogViewerViewModel(DataSourceViewModel dataSource, IDispatcher dispatcher, TimeSpan maximumWaitTime)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");

			_maximumWaitTime = maximumWaitTime;
			_dataSource = dataSource;
			_dataSource.PropertyChanged += DataSourceOnPropertyChanged;

			_dispatcher = dispatcher;
			_fullLogFile = dataSource.DataSource.LogFile;

			_pendingSections = new List<KeyValuePair<ILogFile, LogFileSection>>();
			_logEntries = new ObservableCollection<LogEntryViewModel>();

			UpdateCurrentLogFile();
		}

		public LogViewerViewModel(DataSourceViewModel dataSource, IDispatcher dispatcher)
			: this(dataSource, dispatcher, TimeSpan.FromMilliseconds(10))
		{
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
			file.AddListener(this, _maximumWaitTime, 1000);
			UpdateCounts();
		}

		private void ClearAllEntries()
		{
			_logEntries.Clear();
		}

		private void UpdateCurrentLogFile()
		{
			var stringFilter = DataSource.StringFilter;
			var levelFilter = DataSource.LevelsFilter;
			var otherFilter = DataSource.OtherFilter;

			ILogFile logFile;
			var filter = Filter.Create(stringFilter, true, levelFilter, otherFilter, _quickFilterChain);
			if (filter != null)
			{
				logFile = _fullLogFile.AsFiltered(filter, _maximumWaitTime);
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

		public ObservableCollection<LogEntryViewModel> LogEntries
		{
			get { return _logEntries; }
		}

		public DataSourceViewModel DataSource
		{
			get { return _dataSource; }
		}

		/// <summary>
		/// The list of filters as produced by the "quick filter" panel.
		/// </summary>
		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _quickFilterChain; }
			set
			{
				if (value == _quickFilterChain)
					return;

				_quickFilterChain = value;
				UpdateCurrentLogFile();
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
				if (section == LogFileSection.Reset)
					_pendingSections.Clear();

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
					if (section == LogFileSection.Reset)
					{
						_logEntries.Clear();
						LogEntryCount = 0;
					}
					else
					{
						var entries = _currentLogFile.GetSection(section);
						for (int i = 0; i < entries.Length; ++i)
						{
							var model = new LogEntryViewModel((int) (section.Index + i), entries[i]);
							_logEntries.Add(model);
						}
					}
				}

				_pendingSections.Clear();
			}

			UpdateCounts();
		}

		private void UpdateCounts()
		{
			LogEntryCount = _currentLogFile.Count;
		}

		public void Dispose()
		{
			_dataSource.PropertyChanged -= DataSourceOnPropertyChanged;
		}

		private void DataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "StringFilter":
				case "LevelsFilter":
				case "OtherFilter":
					UpdateCurrentLogFile();
					break;
			}
		}
	}
}