using System;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
{
	internal sealed class DataSource
	{
		private readonly DataSourceSettings _settings;

		public DateTime LastOpened;
		public DateTime LastWritten;

		public DataSource(DataSourceSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
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
	}
}