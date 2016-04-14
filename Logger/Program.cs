using System.Reflection;
using System.Threading;
using log4net;

namespace Logger
{
	internal class Program
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static void Main(string[] args)
		{
			using (var logger = new Tailviewer.Test.Logger("Slow.log"))
			{
				while (true)
				{
					for (int i = 0; i < 9; ++i)
					{
						Log.InfoFormat("Test");
						Thread.Sleep(10);
					}
					Log.WarnFormat("Shit");
				}
			}
		}
	}
}