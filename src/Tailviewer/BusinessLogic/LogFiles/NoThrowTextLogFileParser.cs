using System;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Plugins;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public sealed class NoThrowTextLogFileParser
		: ITextLogFileParser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITextLogFileParser _inner;

		public NoThrowTextLogFileParser(ITextLogFileParser inner)
		{
			_inner = inner;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			try
			{
				_inner.Dispose();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		#endregion

		#region Implementation of ITextLogFileParser

		public IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry)
		{
			try
			{
				return _inner.Parse(logEntry);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return logEntry;
			}
		}

		#endregion
	}
}