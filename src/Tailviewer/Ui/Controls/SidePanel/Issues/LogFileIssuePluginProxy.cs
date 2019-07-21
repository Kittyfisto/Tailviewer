using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins.Issues;

namespace Tailviewer.Ui.Controls.SidePanel.Issues
{
	internal sealed class LogFileIssuePluginProxy
		: ILogFileIssuesPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileIssuesPlugin _inner;

		public LogFileIssuePluginProxy(ILogFileIssuesPlugin inner)
		{
			_inner = inner;
		}

		#region Implementation of ILogFileIssuesPlugin

		public IReadOnlyList<Regex> SupportedFileNames
		{
			get
			{
				try
				{
					return _inner.SupportedFileNames;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception: {0}", e);
					return new Regex[0];
				}
			}
		}

		public ILogFileIssueAnalyser CreateAnalyser(IServiceContainer services, ILogFile logFile)
		{
			try
			{
				var analyser = _inner.CreateAnalyser(services, logFile);
				return new LogFileIssueAnalyserProxy(analyser);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new LogFileIssueAnalyserDummy();
			}
		}

		#endregion
	}
}