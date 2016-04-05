using System;
using System.Threading;

namespace Tailviewer.Test.BusinessLogic
{
	public class AbstractTest
	{
		public static bool WaitUntil(Func<bool> fn, TimeSpan timeout)
		{
			DateTime started = DateTime.UtcNow;
			while ((DateTime.UtcNow - started) < timeout)
			{
				if (fn())
					return true;

				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}

			return false;
		}
	}
}