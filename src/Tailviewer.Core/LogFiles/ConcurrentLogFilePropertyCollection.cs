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
		public ConcurrentLogFilePropertyCollection(params ILogFilePropertyDescriptor[] propertiesDescriptor)
			: this((IEnumerable<ILogFilePropertyDescriptor>) propertiesDescriptor)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		public ConcurrentLogFilePropertyCollection(IEnumerable<ILogFilePropertyDescriptor> properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			_syncRoot = new object();
			_storage = new LogFilePropertyList(properties);
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFilePropertyDescriptor> Properties
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
		public void SetValue(ILogFilePropertyDescriptor property, object value)
		{
			lock (_syncRoot)
			{
				_storage.SetValue(property, value);
			}
		}

		/// <inheritdoc />
		public void SetValue<T>(ILogFilePropertyDescriptor<T> property, T value)
		{
			lock (_syncRoot)
			{
				_storage.SetValue(property, value);
			}
		}

		/// <inheritdoc />
		public bool TryGetValue(ILogFilePropertyDescriptor property, out object value)
		{
			lock (_syncRoot)
			{
				return _storage.TryGetValue(property, out value);
			}
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(ILogFilePropertyDescriptor<T> property, out T value)
		{
			lock (_syncRoot)
			{
				return _storage.TryGetValue(property, out value);
			}
		}

		/// <inheritdoc />
		public object GetValue(ILogFilePropertyDescriptor property)
		{
			lock (_syncRoot)
			{
				return _storage.GetValue(property);
			}
		}

		/// <inheritdoc />
		public T GetValue<T>(ILogFilePropertyDescriptor<T> property)
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