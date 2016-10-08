using System;
using System.Reflection;
using log4net;

namespace Tailviewer.BusinessLogic.Scheduling
{
	internal sealed class PeriodicTask
		: IPeriodicTask
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Action _callback;
		private readonly TimeSpan _minimumWaitTime;
		private readonly string _name;
		private DateTime _lastInvocation;

		public PeriodicTask(Action callback, TimeSpan minimumWaitTime, string name = null)
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			_callback = callback;
			_minimumWaitTime = minimumWaitTime;
			_name = name;
		}

		public TimeSpan MinimumWaitTime
		{
			get { return _minimumWaitTime; }
		}

		public DateTime LastInvocation
		{
			get { return _lastInvocation; }
		}

		/// <summary>
		/// </summary>
		public bool IsRemoved { get; set; }

		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		///     Tests if this task should run now.
		/// </summary>
		/// <param name="now"></param>
		/// <param name="remainingWaitTime"></param>
		/// <returns></returns>
		public bool ShouldRun(DateTime now, out TimeSpan remainingWaitTime)
		{
			remainingWaitTime = now - _lastInvocation;
			return remainingWaitTime >= _minimumWaitTime;
		}

		public void Run()
		{
			try
			{
				_callback();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
			finally
			{
				_lastInvocation = DateTime.Now;
			}
		}
	}
}