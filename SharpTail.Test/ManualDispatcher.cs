using System;
using System.Collections.Generic;
using System.Windows.Threading;
using SharpTail.Ui;

namespace SharpTail.Test
{
	public sealed class ManualDispatcher
		: IDispatcher
	{
		private readonly SortedDictionary<DispatcherPriority, List<Action>> _pendingInvokes;

		public ManualDispatcher()
		{
			_pendingInvokes = new SortedDictionary<DispatcherPriority, List<Action>>();
		}

		public void InvokeAll()
		{
			lock (_pendingInvokes)
			{
				foreach (var pair in _pendingInvokes)
				{
					var invokes = pair.Value;
					foreach (var invoke in invokes)
					{
						invoke();
					}
					invokes.Clear();
				}
			}
		}

		public void BeginInvoke(Action fn)
		{
			BeginInvoke(fn, DispatcherPriority.Normal);
		}

		public void BeginInvoke(Action fn, DispatcherPriority priority)
		{
			lock (_pendingInvokes)
			{
				List<Action> invokes;
				if (!_pendingInvokes.TryGetValue(priority, out invokes))
				{
					invokes = new List<Action>();
					_pendingInvokes.Add(priority, invokes);
				}

				invokes.Add(fn);
			}
		}
	}
}