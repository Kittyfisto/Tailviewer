using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Holds a list of log file properties.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogFilePropertiesDebuggerView))]
	public sealed class ConcurrentLogFilePropertyCollection
		: ILogFileProperties
	{
		private readonly object _syncRoot;
		private readonly LogFilePropertyList _storage;

		/// <summary>
		/// </summary>
		/// <param name="propertiesDescriptor"></param>
		public ConcurrentLogFilePropertyCollection(params IReadOnlyPropertyDescriptor[] propertiesDescriptor)
			: this((IEnumerable<IReadOnlyPropertyDescriptor>) propertiesDescriptor)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		public ConcurrentLogFilePropertyCollection(IEnumerable<IReadOnlyPropertyDescriptor> properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			_syncRoot = new object();
			_storage = new LogFilePropertyList(properties);
		}

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get
			{
				lock (_syncRoot)
				{
					return _storage.Properties.ToList();
				}
			}
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileProperties properties)
		{
			lock (_syncRoot)
			{
				_storage.CopyFrom(properties);
			}
		}

		/// <inheritdoc />
		public bool SetValue(IReadOnlyPropertyDescriptor property, object value)
		{
			lock (_syncRoot)
			{
				return _storage.SetValue(property, value);
			}
		}

		/// <inheritdoc />
		public bool SetValue<T>(IReadOnlyPropertyDescriptor<T> property, T value)
		{
			lock (_syncRoot)
			{
				return _storage.SetValue(property, value);
			}
		}

		/// <inheritdoc />
		public bool TryGetValue(IReadOnlyPropertyDescriptor property, out object value)
		{
			lock (_syncRoot)
			{
				return _storage.TryGetValue(property, out value);
			}
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(IReadOnlyPropertyDescriptor<T> property, out T value)
		{
			lock (_syncRoot)
			{
				return _storage.TryGetValue(property, out value);
			}
		}

		/// <inheritdoc />
		public object GetValue(IReadOnlyPropertyDescriptor property)
		{
			lock (_syncRoot)
			{
				return _storage.GetValue(property);
			}
		}

		/// <inheritdoc />
		public T GetValue<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			lock (_syncRoot)
			{
				return _storage.GetValue(property);
			}
		}

		/// <inheritdoc />
		public void CopyAllValuesTo(ILogFileProperties destination)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			lock (_syncRoot)
			{
				_storage.CopyAllValuesTo(destination);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Reset()
		{
			lock (_syncRoot)
			{
				_storage.Reset();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			lock (_syncRoot)
			{
				_storage.Clear();
			}
		}
	}
}