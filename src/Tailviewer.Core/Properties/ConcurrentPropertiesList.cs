using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tailviewer.Api;

namespace Tailviewer.Core.Properties
{
	/// <summary>
	///     Holds a list of log file properties.
	/// </summary>
	[DebuggerTypeProxy(typeof(PropertiesBufferDebuggerVisualization))]
	public sealed class ConcurrentPropertiesList
		: IPropertiesBuffer
	{
		private readonly object _syncRoot;
		private readonly PropertiesBufferList _storage;

		/// <summary>
		/// </summary>
		/// <param name="propertiesDescriptor"></param>
		public ConcurrentPropertiesList(params IReadOnlyPropertyDescriptor[] propertiesDescriptor)
			: this((IEnumerable<IReadOnlyPropertyDescriptor>) propertiesDescriptor)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		public ConcurrentPropertiesList(IEnumerable<IReadOnlyPropertyDescriptor> properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			_syncRoot = new object();
			_storage = new PropertiesBufferList(properties);
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
		public void CopyFrom(IPropertiesBuffer properties)
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
		public void CopyAllValuesTo(IPropertiesBuffer destination)
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
				_storage.SetToDefault();
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public bool Contains(IReadOnlyPropertyDescriptor property)
		{
			lock (_syncRoot)
			{
				return _storage.Properties.Contains(property);
			}
		}
	}
}