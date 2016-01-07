using System;

namespace Tailviewer.BusinessLogic
{
	internal sealed class DataSource
		: IDisposable
	{
		private readonly LogFile _logFile;
		private readonly Settings.DataSource _settings;

		public DateTime LastOpened;
		public DateTime LastWritten { get { return _logFile.LastWritten; } }

		public DataSource(Settings.DataSource settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
			_logFile = new LogFile(settings.File);
			_logFile.Start();
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

		public int OtherCount
		{
			get { return _logFile.OtherCount; }
		}

		public int DebugCount
		{
			get { return _logFile.DebugCount; }
		}

		public int InfoCount
		{
			get { return _logFile.InfoCount; }
		}

		public int WarningCount
		{
			get { return _logFile.WarningCount; }
		}

		public int ErrorCount
		{
			get { return _logFile.ErrorCount; }
		}

		public int FatalCount
		{
			get { return _logFile.FatalCount; }
		}

		public LogFile LogFile
		{
			get { return _logFile; }
		}

		public string FullFileName
		{
			get { return _settings.File; }
			set { _settings.File = value; }
		}

		public bool IsOpen
		{
			get { return _settings.IsOpen; }
			set { _settings.IsOpen = value; }
		}

		public bool FollowTail
		{
			get { return _settings.FollowTail; }
			set { _settings.FollowTail = value; }
		}

		public string StringFilter
		{
			get { return _settings.StringFilter; }
			set { _settings.StringFilter = value; }
		}

		public LevelFlags LevelFilter
		{
			get { return _settings.LevelFilter; }
			set { _settings.LevelFilter = value; }
		}

		public LogEntryIndex SelectedLogEntry
		{
			get { return _settings.SelectedLogEntry; }
			set { _settings.SelectedLogEntry = value; }
		}

		public LogEntryIndex VisibleLogEntry
		{
			get { return _settings.VisibleLogEntry; }
			set { _settings.VisibleLogEntry = value; }
		}

		internal Settings.DataSource Settings
		{
			get { return _settings; }
		}

		public int TotalCount
		{
			get { return _logFile.Count; }
		}

		public Size FileSize
		{
			get { return _logFile.FileSize; }
		}

		public bool OtherFilter
		{
			get { return _settings.ExcludeOther; }
			set { _settings.ExcludeOther = value; }
		}

		public bool ColorByLevel
		{
			get { return _settings.ColorByLevel; }
			set { _settings.ColorByLevel = value; }
		}

		public void Dispose()
		{
			_logFile.Dispose();
		}
	}
}