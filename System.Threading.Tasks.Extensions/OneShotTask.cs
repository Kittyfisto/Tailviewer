using System.Reflection;
using log4net;

namespace System.Threading.Tasks
{
	internal sealed class OneShotTask
		: IOneShotTask
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Action _fn;
		private readonly string _name;
		private readonly DateTime _started;
		private bool _isFinished;
		private bool _isFaulted;

		public OneShotTask(string name, Action fn)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (fn == null)
				throw new ArgumentNullException(nameof(fn));

			_name = name;
			_fn = fn;
			_started = DateTime.Now;
		}

		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		///     Executes this task.
		/// </summary>
		public void Run()
		{
			try
			{
				_fn();
				_isFinished = true;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				_isFaulted = true;
			}
		}

		public bool IsFinished
		{
			get { return _isFinished; }
		}

		public bool IsFaulted
		{
			get { return _isFaulted; }
		}

		public TimeSpan RemainingTimeUntilInvocation
		{
			get
			{
				var remaining = DateTime.Now - _started;
				if (remaining < TimeSpan.Zero)
					return TimeSpan.Zero;

				return remaining;
			}
		}
	}
}