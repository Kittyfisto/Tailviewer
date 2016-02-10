using System;

namespace Tailviewer.BusinessLogic
{
	internal abstract class AbstractDataSource
		: IDataSource
	{
		private readonly Settings.DataSource _settings;

		public DateTime LastModified { get { return LogFile.LastModified; } }

		public DateTime LastViewed
		{
			get { return _settings.LastViewed; }
			set { _settings.LastViewed = value; }
		}

		protected AbstractDataSource(Settings.DataSource settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
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

		public Settings.DataSource Settings
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
	}
}