using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     Replaces the log source which is forwarded to every
	///     <see cref="ILogSourceListener.OnLogFileModified(ILogSource, LogFileSection)" /> invocation
	///     with the proxy reference, given during construction.
	/// </summary>
	internal sealed class ProxyLogListenerCollection
	{
		private readonly ILogSource _actualSource;
		private readonly Dictionary<ILogSourceListener, ListenerProxy> _listeners;
		private readonly ILogSource _proxy;

		private readonly object _syncRoot;

		public ProxyLogListenerCollection(ILogSource actualSource, ILogSource proxy)
		{
			_actualSource = actualSource;
			_proxy = proxy;
			_syncRoot = new object();
			_listeners = new Dictionary<ILogSourceListener, ListenerProxy>();
		}

		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			// We need to make sure that whoever registers with us is getting OUR reference through
			// their listener, not the source we're wrapping (or they might discard events since they're
			// coming not from the source they subscribed to).
			var proxy = new ListenerProxy(_proxy, listener);
			lock (_syncRoot)
			{
				_listeners.Add(listener, proxy);
			}

			_actualSource.AddListener(proxy, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogSourceListener listener)
		{
			ListenerProxy proxy;
			lock (_syncRoot)
			{
				if (!_listeners.TryGetValue(listener, out proxy))
					return;
			}

			_actualSource.RemoveListener(proxy);
		}
	}
}