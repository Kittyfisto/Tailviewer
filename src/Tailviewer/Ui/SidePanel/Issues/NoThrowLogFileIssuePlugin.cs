using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.Plugins;

namespace Tailviewer.Ui.SidePanel.Issues
{
	internal sealed class NoThrowLogFileIssuePlugin
		: ILogFileIssuesPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileIssuesPlugin _inner;

		public NoThrowLogFileIssuePlugin(ILogFileIssuesPlugin inner)
		{
			_inner = inner;
		}

		#region Implementation of ILogFileIssuesPlugin

		public IReadOnlyList<ILogFileFormat> SupportedFormats
		{
			get
			{
				try
				{
					return _inner.SupportedFormats;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception: {0}", e);
					return new ILogFileFormat[0];
				}
			}
		}

		public ILogSourceIssueAnalyser CreateAnalyser(IServiceContainer services, ILogSource logSource)
		{
			try
			{
				var analyser = _inner.CreateAnalyser(services, logSource);
				return new NoThrowLogSourceIssueAnalyser(analyser);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new LogSourceIssueAnalyserDummy();
			}
		}

		#endregion
	}
}