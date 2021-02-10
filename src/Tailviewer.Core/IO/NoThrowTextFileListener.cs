using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.IO
{
	internal sealed class NoThrowTextFileListener
		: ITextFileListener
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITextFileListener _listener;

		public NoThrowTextFileListener(ITextFileListener listener)
		{
			_listener = listener;
		}

		#region Implementation of ITextFileListener

		public void OnReset(ILogFileProperties properties)
		{
			try
			{
				_listener.OnReset(properties);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}
		}

		public void OnEndOfSourceReached(ILogFileProperties properties)
		{
			try
			{
				_listener.OnEndOfSourceReached(properties);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}
		}

		public void OnRead(ILogFileProperties properties, LogFileSection readSection, IReadOnlyList<string> lines)
		{
			try
			{
				_listener.OnRead(properties, readSection, lines);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}
		}

		#endregion
	}
}