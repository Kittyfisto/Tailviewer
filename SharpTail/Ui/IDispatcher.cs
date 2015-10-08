using System;
using System.Windows.Threading;

namespace SharpTail.Ui
{
	public interface IDispatcher
	{
		void BeginInvoke(Action fn);
		void BeginInvoke(Action fn, DispatcherPriority priority);
	}
}