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
		private readonly IDataSourceViewModel _dataSource;
		private readonly IDispatcher _dispatcher;
		private readonly ObservableCollection<LogEntryViewModel> _logEntries;
		private readonly TimeSpan _maximumWaitTime;
		private readonly List<KeyValuePair<ILogFile, LogFileSection>> _pendingSections;
		private ILogFile _currentLogFile;
		private int _logEntryCount;
		private IEnumerable<ILogEntryFilter> _quickFilterChain;
		private int _totalLogEntryCount;

		public LogViewerViewModel(IDataSourceViewModel dataSource, IDispatcher dispatcher, TimeSpan maximumWaitTime)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");

			_maximumWaitTime = maximumWaitTime;
			_dataSource = dataSource;
			_dataSource.PropertyChanged += DataSourceOnPropertyChanged;

			_dispatcher = dispatcher;

			_pendingSections = new List<KeyValuePair<ILogFile, LogFileSection>>();
			_logEntries = new ObservableCollection<LogEntryViewModel>();

			SetCurrentLogFile(null, _dataSource.DataSource.FilteredLogFile);
		}

		public LogViewerViewModel(IDataSourceViewModel dataSource, IDispatcher dispatcher)
			: this(dataSource, dispatcher, TimeSpan.FromMilliseconds(10))
		{
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

		public int TotalLogEntryCount
		{
			get { return _totalLogEntryCount; }
			private set
			{
				if (value == _totalLogEntryCount)
					return;

				_totalLogEntryCount = value;
				EmitPropertyChanged();
			}
		}

		public ObservableCollection<LogEntryViewModel> LogEntries
		{
			get { return _logEntries; }
		}

		public IDataSourceViewModel DataSource
		{
			get { return _dataSource; }
		}

		/// <summary>
		///     The list of filters as produced by the "quick filter" panel.
		/// </summary>
		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _quickFilterChain; }
			set
			{
				if (value == _quickFilterChain)
					return;

				_quickFilterChain = value;
				SetCurrentLogFile(_currentLogFile, _dataSource.DataSource.FilteredLogFile);
			}
		}

		public void Dispose()
		{
			_dataSource.PropertyChanged -= DataSourceOnPropertyChanged;
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			lock (_pendingSections)
			{
				if (section == LogFileSection.Reset)
					_pendingSections.Clear();

				_pendingSections.Add(new KeyValuePair<ILogFile, LogFileSection>(_currentLogFile, section));
				_dispatcher.BeginInvoke(Synchronize, DispatcherPriority.Background);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void SetCurrentLogFile(ILogFile oldLogFile, ILogFile newLogFile)
		{
			if (oldLogFile == newLogFile)
				return;

			ClearAllEntries();

			if (oldLogFile != null)
			{
				oldLogFile.Remove(this);
			}

			_currentLogFile = newLogFile;

			if (newLogFile != null)
			{
				newLogFile.AddListener(this, _maximumWaitTime, 1000);
			}

			UpdateCounts();
		}

		private void ClearAllEntries()
		{
			_logEntries.Clear();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Synchronize()
		{
			lock (_pendingSections)
			{
				foreach (var pair in _pendingSections)
				{
					ILogFile file = pair.Key;
					if (file != _currentLogFile)
						continue; //< This message belongs to an old change and must be ignored

					LogFileSection section = pair.Value;
					if (section.IsReset)
					{
						_logEntries.Clear();
						LogEntryCount = 0;
						TotalLogEntryCount = 0;
					}
					else if (section.InvalidateSection)
					{
						for (int i = 0; i < section.Count; ++i)
						{
							_logEntries.RemoveAt((int) section.Index);
						}
					}
					else
					{
						LogLine[] entries = _currentLogFile.GetSection(section);
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
			TotalLogEntryCount = _dataSource.DataSource.LogFile.Count;
		}

		private void DataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "StringFilter":
				case "LevelsFilter":
				case "OtherFilter":
					SetCurrentLogFile(_currentLogFile, _dataSource.DataSource.FilteredLogFile);
					break;
			}
		}
	}
}