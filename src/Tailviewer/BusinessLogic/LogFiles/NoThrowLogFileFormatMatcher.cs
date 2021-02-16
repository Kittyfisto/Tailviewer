using System;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Plugins;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Responsible for handling all exceptions which might be thrown by a <see cref="ILogFileFormatMatcher" />
	///     implementation.
	/// </summary>
	internal class NoThrowLogFileFormatMatcher
		: ILogFileFormatMatcher
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileFormatMatcher _inner;

		public NoThrowLogFileFormatMatcher(ILogFileFormatMatcherPlugin plugin, IServiceContainer services)
		{
			try
			{
				_inner = plugin.CreateMatcher(services);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		#region Implementation of ILogFileFormatMatcher

		public bool TryMatchFormat(string fileName, byte[] initialContent, out ILogFileFormat format)
		{
			try
			{
				if (_inner == null)
				{
					format = null;
					return false;
				}

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