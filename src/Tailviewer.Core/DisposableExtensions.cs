using System;
using System.Reflection;
using log4net;

namespace Tailviewer.Core
{
	internal static class DisposableExtensions
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void TryDispose(this IDisposable that)
		{
			try
			{
				that.Dispose();
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}
		}
	}
}