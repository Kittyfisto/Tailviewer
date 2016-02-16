using System;
using System.Collections.Generic;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
{
	internal abstract class AbstractDataSource
		: IDataSource
	{
		private readonly TimeSpan _maximumWaitTime;
		private readonly DataSource _settings;
		private ILogFile _filteredLogFile;
		private IEnumerable<ILogEntryFilter> _quickFilterChain;

		protected AbstractDataSource(DataSource settings, TimeSpan maximumWaitTime)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (settings.Id == Guid.Empty) throw new ArgumentException("settings.Id shall be set to an actually generated id");

			_settings = settings;
			_maximumWaitTime = maximumWaitTime;
		}

		public override string ToString()
		{
			return _settings.ToString();
		}

		public ILogFile FilteredLogFile
		{
			get { return _filteredLogFile; }
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
				CreateFilteredLogFile();
			}
		}

		public string StringFilter
		{
			get { return _settings.StringFilter; }
			set
			{
				if (value == StringFilter)
					return;

				_settings.StringFilter = value;
				CreateFilteredLogFile();
			}
		}

		public LevelFlags LevelFilter
		{
			get { return _settings.LevelFilter; }
			set
			{
				if (value == LevelFilter)
					return;

				_settings.LevelFilter = value;
				CreateFilteredLogFile();
			}
		}

		public DateTime LastModified
		{
			get { return LogFile.LastModified; }
		}

		public DateTime LastViewed
		{
			get { return _settings.LastViewed; }
			set { _settings.LastViewed = value; }
		}

		public Guid Id
		{
			get { return _settings.Id; }
		}

		public Guid ParentId
		{
			get { return _settings.ParentId; }
		}

		public void ActivateQuickFilter(Guid id)
		{
			// Should I add a sanity check here?
			_settings.ActivatedQuickFilters.Add(id);
		}

		public bool DeactivateQuickFilter(Guid id)
		{
			return _settings.ActivatedQuickFilters.Remove(id);
		}

		public bool IsQuickFilterActive(Guid id)
		{
			return _settings.ActivatedQuickFilters.Contains(id);
		}

		public abstract ILogFile LogFile { get; }

		public int OtherCount
		{
			get { return LogFile.OtherCount; }
		}

		public int DebugCount
		{
			get { return LogFile.DebugCount; }
		}

		public int InfoCount
		{
			get { return LogFile.InfoCount; }
		}

		public int WarningCount
		{
			get { return LogFile.WarningCount; }
		}

		public int ErrorCount
		{
			get { return LogFile.ErrorCount; }
		}

		public int FatalCount
		{
			get { return LogFile.FatalCount; }
		}

		public string FullFileName
		{
			get { return _settings.File; }
			set { _settings.File = value; }
		}

		public bool FollowTail
		{
			get { return _settings.FollowTail; }
			set { _settings.FollowTail = value; }
		}

		public LogLineIndex SelectedLogLine
		{
			get { return _settings.SelectedLogLine; }
			set { _settings.SelectedLogLine = value; }
		}

		public LogLineIndex VisibleLogLine
		{
			get { return _settings.VisibleLogLine; }
			set { _settings.VisibleLogLine = value; }
		}

		public DataSource Settings
		{
			get { return _settings; }
		}

		public int TotalCount
		{
			get { return LogFile.Count; }
		}

		public Size FileSize
		{
			get { return LogFile.FileSize; }
		}

		public bool ColorByLevel
		{
			get { return _settings.ColorByLevel; }
			set { _settings.ColorByLevel = value; }
		}

		public void Dispose()
		{
			LogFile.Dispose();
		}

		protected void CreateFilteredLogFile()
		{
			string stringFilter = StringFilter;
			LevelFlags levelFilter = LevelFilter;

			ILogFile newLogFile;
			ILogEntryFilter filter = Filter.Create(stringFilter, true, levelFilter, _quickFilterChain);
			if (filter != null)
			{
				newLogFile = LogFile.AsFiltered(filter, _maximumWaitTime);
			}
			else
			{
				newLogFile = LogFile;
			}

			if (_filteredLogFile != null)
			{
				if (_filteredLogFile != LogFile)
					_filteredLogFile.Dispose();
			}

			_filteredLogFile = newLogFile;
		}
	}
}