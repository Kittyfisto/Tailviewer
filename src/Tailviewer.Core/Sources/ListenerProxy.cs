using Tailviewer.Api;

namespace Tailviewer.Core.Sources
{
	internal sealed class ListenerProxy
		: ILogSourceListener
	{
		private readonly ILogSourceListener _listener;
		private readonly ILogSource _source;

		public ListenerProxy(ILogSource source, ILogSourceListener listener)
		{
			_source = source;
			_listener = listener;
		}


		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			_listener.OnLogFileModified(_source, modification);
		}

		#endregion
	}
}