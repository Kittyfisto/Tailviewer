using System.Security.Permissions;
using System.Windows.Threading;

namespace Tailviewer.Test.Ui.Controls
{
	public static class DispatcherExtensions
	{
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ExecuteAllEvents()
		{
			var frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
			                                         new DispatcherOperationCallback(ExitFrame), frame);
			Dispatcher.PushFrame(frame);
		}

		private static object ExitFrame(object frame)
		{
			((DispatcherFrame)frame).Continue = false;
			return null;
		}
	}
}