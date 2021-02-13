using System;
using System.Collections;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Responsible for enumerating <see cref="ILogEntries" />.
	///     Used by its implementations.
	/// </summary>
	internal sealed class LogEntriesEnumerator
		: IEnumerator<ILogEntry>
	{
		private readonly ILogEntries _logEntries;
		private int _index;

		public LogEntriesEnumerator(ILogEntries logEntries)
		{
			_logEntries = logEntries;
			Reset();
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (++_index >= _logEntries.Count)
				return false;

			return true;
		}

		public void Reset()
		{
			_index = -1;
		}

		public ILogEntry Current
		{
			get
			{
				if (_index < 0 || _index >= _logEntries.Count)
					throw new InvalidOperationException();

				return _logEntries[_index];
			}
		}

		object IEnumerator.Current => Current;
	}
}