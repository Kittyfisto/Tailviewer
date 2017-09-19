using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.Filters;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	public class AnalysisDataSource
		: IDataSource
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public ILogFile UnfilteredLogFile
		{
			get { throw new NotImplementedException(); }
		}

		public ILogFile FilteredLogFile
		{
			get { throw new NotImplementedException(); }
		}

		public ILogFileSearch Search
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LastModified
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LastViewed
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string FullFileName
		{
			get { throw new NotImplementedException(); }
		}

		public bool FollowTail
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool ShowLineNumbers
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string SearchTerm
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public LevelFlags LevelFilter
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public HashSet<LogLineIndex> SelectedLogLines
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public LogLineIndex VisibleLogLine
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public double HorizontalOffset
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public DataSource Settings
		{
			get { throw new NotImplementedException(); }
		}

		public int TotalCount
		{
			get { throw new NotImplementedException(); }
		}

		public Size FileSize
		{
			get { throw new NotImplementedException(); }
		}

		public bool ColorByLevel
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool HideEmptyLines
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool IsSingleLine
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public Guid Id
		{
			get { throw new NotImplementedException(); }
		}

		public Guid ParentId
		{
			get { throw new NotImplementedException(); }
		}

		public int NoLevelCount
		{
			get { throw new NotImplementedException(); }
		}

		public int DebugCount
		{
			get { throw new NotImplementedException(); }
		}

		public int InfoCount
		{
			get { throw new NotImplementedException(); }
		}

		public int WarningCount
		{
			get { throw new NotImplementedException(); }
		}

		public int ErrorCount
		{
			get { throw new NotImplementedException(); }
		}

		public int FatalCount
		{
			get { throw new NotImplementedException(); }
		}

		public int NoTimestampCount
		{
			get { throw new NotImplementedException(); }
		}

		public void ActivateQuickFilter(Guid id)
		{
			throw new NotImplementedException();
		}

		public bool DeactivateQuickFilter(Guid id)
		{
			throw new NotImplementedException();
		}

		public bool IsQuickFilterActive(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}