using System;
using System.Collections;
using System.Collections.Generic;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Responsible for enumerating <see cref="ILogBuffer" />.
	///     Used by its implementations.
	/// </summary>
	internal sealed class LogBufferEnumerator
		: IEnumerator<ILogEntry>
	{
		private readonly ILogBuffer _logBuffer;
		private int _index;

		public LogBufferEnumerator(ILogBuffer logBuffer)
		{
			_logBuffer = logBuffer;
			Reset();
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (++_index >= _logBuffer.Count)
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
				if (_index < 0 || _index >= _logBuffer.Count)
					throw new InvalidOperationException();

				return _logBuffer[_index];
			}
		}

		object IEnumerator.Current => Current;
	}
}