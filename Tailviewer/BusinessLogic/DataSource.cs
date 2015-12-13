using System;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
{
	internal sealed class DataSource
		: IDisposable
	{
		private readonly LogFile _logFile;
		private readonly DataSourceSettings _settings;

		public DateTime LastOpened;
		public DateTime LastWritten;

		public DataSource(DataSourceSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
			_logFile = new LogFile(settings.File);
			_logFile.Start();
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

		internal DataSourceSettings Settings
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

		public void Dispose()
		{
			_logFile.Dispose();
		}
	}
}