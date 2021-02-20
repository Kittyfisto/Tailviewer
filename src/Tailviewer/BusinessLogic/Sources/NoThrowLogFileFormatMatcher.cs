using System;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Tailviewer.Plugins;

namespace Tailviewer.BusinessLogic.Sources
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

		public bool TryMatchFormat(string fileName,
		                           Stream fileStream,
		                           Encoding encoding,
		                           out ILogFileFormat format)
		{
			try
			{
				if (_inner == null)
				{
					format = null;
					return false;
				}

				return _inner.TryMatchFormat(fileName, fileStream, encoding, out format);
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