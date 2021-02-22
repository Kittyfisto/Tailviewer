using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class LogFileSearchListenerCollection
	{
		private readonly ILogSourceSearch _sender;
		private readonly List<ILogFileSearchListener> _listeners;
		private List<LogMatch> _matches;

		public LogFileSearchListenerCollection(ILogSourceSearch sender)
		{
			if (sender == null)
				throw new ArgumentNullException(nameof(sender));

			_sender = sender;
			_listeners = new List<ILogFileSearchListener>();
			_matches = new List<LogMatch>();
		}

		public void AddListener(ILogFileSearchListener listener)
		{
			lock (_listeners)
			{
				_listeners.Add(listener);
				listener.OnSearchModified(_sender, _matches);
			}
		}

		public void RemoveListener(ILogFileSearchListener listener)
		{
			lock (_listeners)
			{
				_listeners.Remove(listener);
			}
		}

		public void EmitSearchChanged(IEnumerable<LogMatch> matches)
		{
			lock (_listeners)
			{
				_matches = matches.ToList();
				foreach (var listener in _listeners)
				{
					listener.OnSearchModified(_sender, _matches);
				}
			}
		}
	}
}