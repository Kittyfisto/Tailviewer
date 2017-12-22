using System;
using System.Collections;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Responsible for enumerating <see cref="IReadOnlyLogEntries" />.
	///     Used by its implementations.
	/// </summary>
	internal sealed class ReadOnlyLogEntriesEnumerator
		: IEnumerator<IReadOnlyLogEntry>
	{
		private readonly IReadOnlyLogEntries _logEntries;
		private int _index;

		public ReadOnlyLogEntriesEnumerator(IReadOnlyLogEntries logEntries)
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

		public IReadOnlyLogEntry Current
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