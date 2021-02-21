using System;
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
		                           byte[] header,
		                           Encoding encoding,
		                           out ILogFileFormat format,
		                           out Certainty certainty)
		{
			try
			{
				if (_inner == null)
				{
					format = null;
					certainty = Certainty.Sure;
					return false;
				}

				return _inner.TryMatchFormat(fileName, header, encoding, out format, out certainty);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				format = null;
				certainty = Certainty.Uncertain;
				return false;
			}
		}

		#endregion
	}
}