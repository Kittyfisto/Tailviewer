using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Holds a list of log file properties.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogFilePropertiesView))]
	public sealed class LogFilePropertyList
		: ILogFileProperties
	{
		private readonly List<ILogFilePropertyDescriptor> _propertyDescriptors;
		private readonly Dictionary<ILogFilePropertyDescriptor, IPropertyStorage> _values;

		/// <summary>
		/// </summary>
		/// <param name="propertiesDescriptor"></param>
		public LogFilePropertyList(params ILogFilePropertyDescriptor[] propertiesDescriptor)
			: this((IEnumerable<ILogFilePropertyDescriptor>) propertiesDescriptor)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		public LogFilePropertyList(IEnumerable<ILogFilePropertyDescriptor> properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			_propertyDescriptors = new List<ILogFilePropertyDescriptor>(properties);
			_values = new Dictionary<ILogFilePropertyDescriptor, IPropertyStorage>();
			foreach (var propertyDescriptor in _propertyDescriptors)
				_values.Add(propertyDescriptor, CreateValueStorage(propertyDescriptor));
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFilePropertyDescriptor> Properties => _propertyDescriptors;

		/// <inheritdoc />
		public void SetValue(ILogFilePropertyDescriptor propertyDescriptor, object value)
		{
			if (propertyDescriptor == null)
				throw new ArgumentNullException(nameof(propertyDescriptor));
			if (!LogFileProperty.IsAssignableFrom(propertyDescriptor, value))
				throw new ArgumentException();

			if (!_values.ContainsKey(propertyDescriptor))
			{
				_propertyDescriptors.Add(propertyDescriptor);
				var storage = CreateValueStorage(propertyDescriptor);
				storage.Value = value;
				_values.Add(propertyDescriptor, storage);
			}
			else
			{
				_values[propertyDescriptor].Value = value;
			}
		}

		/// <inheritdoc />
		public void SetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor, T value)
		{
			if (propertyDescriptor == null)
				throw new ArgumentNullException(nameof(propertyDescriptor));
			if (!LogFileProperty.IsAssignableFrom(propertyDescriptor, value))
				throw new ArgumentException();

			if (!_values.ContainsKey(propertyDescriptor))
			{
				_propertyDescriptors.Add(propertyDescriptor);
				var storage = CreateValueStorage(propertyDescriptor);
				storage.Value = value;
				_values.Add(propertyDescriptor, storage);
			}
			else
			{
				((PropertyStorage<T>)_values[propertyDescriptor]).Value = value;
			}
		}

		/// <inheritdoc />
		public bool TryGetValue(ILogFilePropertyDescriptor propertyDescriptor, out object value)
		{
			IPropertyStorage storage;
			if (!_values.TryGetValue(propertyDescriptor, out storage))
			{
				value = propertyDescriptor.DefaultValue;
				return false;
			}

			value = storage.Value;
			return true;
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor, out T value)
		{
			IPropertyStorage storage;
			if (!_values.TryGetValue(propertyDescriptor, out storage))
			{
				value = propertyDescriptor.DefaultValue;
				return false;
			}

			value = ((PropertyStorage<T>)storage).Value;
			return true;
		}

		/// <inheritdoc />
		public object GetValue(ILogFilePropertyDescriptor property)
		{
			object value;
			if (!TryGetValue(property, out value))
				throw new NoSuchPropertyException(property);

			return value;
		}

		/// <inheritdoc />
		public T GetValue<T>(ILogFilePropertyDescriptor<T> property)
		{
			T value;
			if (!TryGetValue(property, out value))
				throw new NoSuchPropertyException(property);

			return value;
		}

		/// <inheritdoc />
		public void GetValues(ILogFileProperties properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			foreach (var propertyDescriptor in properties.Properties)
			{
				object value;
				if (TryGetValue(propertyDescriptor, out value))
					properties.SetValue(propertyDescriptor, value);
			}
		}

		private IPropertyStorage CreateValueStorage(ILogFilePropertyDescriptor propertyDescriptor)
		{
			dynamic tmp = propertyDescriptor;
			return CreateValueStorage(tmp);
		}

		private IPropertyStorage CreateValueStorage<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			return new PropertyStorage<T>(propertyDescriptor.DefaultValue);
		}

		/// <summary>
		///     Responsible for storing the value of a property.
		/// </summary>
		private interface IPropertyStorage
		{
			object Value { get; set; }
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private sealed class PropertyStorage<T>
			: IPropertyStorage
		{
			private T _value;

			public PropertyStorage(T value)
			{
				_value = value;
			}

			public T Value
			{
				get { return _value; }
				set { _value = value; }
			}

			object IPropertyStorage.Value
			{
				get { return Value; }
				set { Value = (T) value; }
			}
		}
	}
}