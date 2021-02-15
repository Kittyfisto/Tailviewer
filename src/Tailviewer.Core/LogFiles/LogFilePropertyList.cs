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
	public sealed class LogFilePropertyList
		: ILogFileProperties
	{
		private readonly List<IReadOnlyPropertyDescriptor> _propertyDescriptors;
		private readonly Dictionary<IReadOnlyPropertyDescriptor, IPropertyStorage> _values;

		/// <summary>
		/// </summary>
		/// <param name="propertiesDescriptor"></param>
		public LogFilePropertyList(params IReadOnlyPropertyDescriptor[] propertiesDescriptor)
			: this((IEnumerable<IReadOnlyPropertyDescriptor>) propertiesDescriptor)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		public LogFilePropertyList(IEnumerable<IReadOnlyPropertyDescriptor> properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			_propertyDescriptors = new List<IReadOnlyPropertyDescriptor>(properties);
			_values = new Dictionary<IReadOnlyPropertyDescriptor, IPropertyStorage>();
			foreach (var propertyDescriptor in _propertyDescriptors)
				_values.Add(propertyDescriptor, CreateValueStorage(propertyDescriptor));
		}

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get
			{
				return _propertyDescriptors;
			}
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileProperties properties)
		{
			foreach (var propertyDescriptor in properties.Properties)
			{
				// By performing a virtual call (CopyFrom) into the typed storage,
				// we can avoid any and all boxing / unboxing of value types.
				if (!_values.ContainsKey(propertyDescriptor))
				{
					_propertyDescriptors.Add(propertyDescriptor);
					var storage = CreateValueStorage(propertyDescriptor);
					storage.CopyFrom(properties);
					_values.Add(propertyDescriptor, storage);
				}
				else
				{
					_values[propertyDescriptor].CopyFrom(properties);
				}
			}
		}

		/// <inheritdoc />
		public void SetValue(IReadOnlyPropertyDescriptor property, object value)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (!LogFileProperty.IsAssignableFrom(property, value))
				throw new ArgumentException();

			if (!_values.ContainsKey(property))
			{
				_propertyDescriptors.Add(property);
				var storage = CreateValueStorage(property);
				storage.Value = value;
				_values.Add(property, storage);
			}
			else
			{
				_values[property].Value = value;
			}
		}

		/// <inheritdoc />
		public void SetValue<T>(IReadOnlyPropertyDescriptor<T> property, T value)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (!LogFileProperty.IsAssignableFrom(property, value))
				throw new ArgumentException();

			if (!_values.ContainsKey(property))
			{
				_propertyDescriptors.Add(property);
				var storage = CreateValueStorage(property);
				storage.Value = value;
				_values.Add(property, storage);
			}
			else
			{
				((PropertyStorage<T>) _values[property]).Value = value;
			}
		}

		/// <inheritdoc />
		public bool TryGetValue(IReadOnlyPropertyDescriptor property, out object value)
		{
			if (!_values.TryGetValue(property, out var storage))
			{
				value = property.DefaultValue;
				return false;
			}

			value = storage.Value;
			return true;
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(IReadOnlyPropertyDescriptor<T> property, out T value)
		{
			if (!_values.TryGetValue(property, out var storage))
			{
				value = property.DefaultValue;
				return false;
			}

			value = ((PropertyStorage<T>) storage).Value;
			return true;
		}

		/// <inheritdoc />
		public object GetValue(IReadOnlyPropertyDescriptor property)
		{
			if (!TryGetValue(property, out var value))
				return property.DefaultValue;

			return value;
		}

		/// <inheritdoc />
		public T GetValue<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			if (!TryGetValue(property, out var value))
				return property.DefaultValue;

			return value;
		}

		/// <inheritdoc />
		public void CopyAllValuesTo(ILogFileProperties destination)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			foreach (var pair in _values)
			{
				pair.Value.CopyTo(destination);
			}
		}

		private IPropertyStorage CreateValueStorage(IReadOnlyPropertyDescriptor propertyDescriptor)
		{
			dynamic tmp = propertyDescriptor;
			return CreateValueStorage(tmp);
		}

		private IPropertyStorage CreateValueStorage<T>(IReadOnlyPropertyDescriptor<T> propertyDescriptor)
		{
			return new PropertyStorage<T>(propertyDescriptor);
		}

		/// <summary>
		///     Responsible for storing the value of a property.
		/// </summary>
		private interface IPropertyStorage
		{
			object Value { get; set; }
			void CopyFrom(ILogFileProperties properties);
			void CopyTo(ILogFileProperties destination);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private sealed class PropertyStorage<T>
			: IPropertyStorage
		{
			private readonly IReadOnlyPropertyDescriptor<T> _property;
			private T _value;

			public PropertyStorage(IReadOnlyPropertyDescriptor<T> property)
			{
				_property = property;
				_value = property.DefaultValue;
			}

			public T Value
			{
				get { return _value; }
				set { _value = value; }
			}

			public void CopyFrom(ILogFileProperties properties)
			{
				_value = properties.GetValue(_property);
			}

			public void CopyTo(ILogFileProperties destination)
			{
				destination.SetValue(_property, _value);
			}

			object IPropertyStorage.Value
			{
				get { return Value; }
				set { Value = (T) value; }
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Reset()
		{
			foreach (var pair in _values)
			{
				pair.Value.Value = pair.Key.DefaultValue;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			_propertyDescriptors.Clear();
			_values.Clear();
		}

		/// <summary>
		/// Adds the given property to this list.
		/// </summary>
		/// <param name="property"></param>
		public void Add<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			if (!_values.ContainsKey(property))
			{
				_values.Add(property, new PropertyStorage<T>(property));
				_propertyDescriptors.Add(property);
			}
		}
	}
}