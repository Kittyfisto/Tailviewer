using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Tailviewer.Ui;

namespace Tailviewer.Test
{
	public sealed class ManualDispatcher
		: IDispatcher
	{
		private readonly SortedDictionary<DispatcherPriority, List<Action>> _pendingInvokes;

		public ManualDispatcher()
		{
			_pendingInvokes = new SortedDictionary<DispatcherPriority, List<Action>>();
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

		public void InvokeAll()
		{
			List<KeyValuePair<DispatcherPriority, List<Action>>> pendingInvokes;

			lock (_pendingInvokes)
			{
				pendingInvokes = _pendingInvokes.Select(x =>
				                                        new KeyValuePair<DispatcherPriority, List<Action>>(
					                                        x.Key, x.Value.ToList()
					                                        )).ToList();
				_pendingInvokes.Clear();
			}

			foreach (var pair in pendingInvokes)
			{
				List<Action> invokes = pair.Value;
				foreach (Action invoke in invokes)
				{
					invoke();
				}
			}
		}
	}
}