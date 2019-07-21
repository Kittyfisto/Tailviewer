using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Responsible for handling all exceptions which might be thrown by a <see cref="ILogFileFormatMatcher" />
	///     implementation.
	/// </summary>
	internal class LogFileFormatMatcherProxy
		: ILogFileFormatMatcher
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileFormatMatcher _inner;

		public LogFileFormatMatcherProxy(ILogFileFormatMatcher inner)
		{
			_inner = inner;
		}

		#region Implementation of ILogFileFormatMatcher

		public bool TryMatchFormat(string fileName, byte[] initialContent, out ILogFileFormat format)
		{
			try
			{
				return _inner.TryMatchFormat(fileName, initialContent, out format);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				format = null;
				return false;
			}
		}

		public bool TryMatchFormat(string fileName, IReadOnlyList<string> initialContent, out ILogFileFormat format)
		{
			try
			{
				return _inner.TryMatchFormat(fileName, initialContent, out format);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				format = null;
				return false;
			}
		}

		#endregion
	}
}