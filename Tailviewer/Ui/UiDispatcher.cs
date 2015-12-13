using System;
using System.Windows.Threading;

namespace Tailviewer.Ui
{
	public sealed class UiDispatcher
		: IDispatcher
	{
		private readonly Dispatcher _dispatcher;

		public UiDispatcher(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void BeginInvoke(Action fn)
		{
			BeginInvoke(fn, DispatcherPriority.Normal);
		}

		public void BeginInvoke(Action fn, DispatcherPriority priority)
		{
			_dispatcher.BeginInvoke(fn, priority);
		}
	}
}