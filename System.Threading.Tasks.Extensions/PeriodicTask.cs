using System.Reflection;
using log4net;

namespace System.Threading.Tasks
{
	internal sealed class PeriodicTask
		: IPeriodicTask
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Action _callback;
		private readonly long _id;
		private readonly TimeSpan _minimumWaitTime;
		private readonly string _name;
		private DateTime _lastInvocation;

		public PeriodicTask(long id, Action callback, TimeSpan minimumWaitTime, string name = null)
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			_id = id;
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

		public long Id
		{
			get { return _id; }
		}

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

		public override string ToString()
		{
			return string.Format("#{0} ({1})", _id, _name);
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