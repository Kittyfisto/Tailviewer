﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Holds a list of log file properties.
	/// </summary>
	[DebuggerTypeProxy(typeof(PropertiesBufferDebuggerVisualization))]
	public sealed class PropertiesBufferList
		: IPropertiesBuffer
	{
		private readonly List<IReadOnlyPropertyDescriptor> _propertyDescriptors;
		private readonly Dictionary<IReadOnlyPropertyDescriptor, IPropertyStorage> _values;

		/// <summary>
		/// </summary>
		/// <param name="propertiesDescriptor"></param>
		public PropertiesBufferList(params IReadOnlyPropertyDescriptor[] propertiesDescriptor)
			: this((IEnumerable<IReadOnlyPropertyDescriptor>) propertiesDescriptor)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		public PropertiesBufferList(IEnumerable<IReadOnlyPropertyDescriptor> properties)
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
		public void CopyFrom(IPropertiesBuffer properties)
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
		public bool SetValue(IReadOnlyPropertyDescriptor property, object value)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (!ReadOnlyPropertyDescriptorExtensions.IsAssignableFrom(property, value))
				throw new ArgumentException();

			if (!_values.ContainsKey(property))
			{
				_propertyDescriptors.Add(property);
				var storage = CreateValueStorage(property);
				bool changed = storage.SetValue(value);
				_values.Add(property, storage);
				return changed;
			}
			else
			{
				return _values[property].SetValue(value);
			}
		}

		/// <inheritdoc />
		public bool SetValue<T>(IReadOnlyPropertyDescriptor<T> property, T value)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (!ReadOnlyPropertyDescriptorExtensions.IsAssignableFrom(property, value))
				throw new ArgumentException();

			if (!_values.ContainsKey(property))
			{
				_propertyDescriptors.Add(property);
				var storage = CreateValueStorage(property);
				bool changed = storage.SetValue(value);
				_values.Add(property, storage);
				return changed;
			}
			else
			{
				return ((PropertyStorage<T>) _values[property]).SetValue(value);
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
		public void CopyAllValuesTo(IPropertiesBuffer destination)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			foreach (var pair in _values)
			{
				pair.Value.CopyTo(destination);
			}
		}

		/// <summary>
		///   Sets only the given properties to their default value.
		/// </summary>
		/// <param name="properties"></param>
		public void SetToDefault(IReadOnlyList<IReadOnlyPropertyDescriptor> properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			foreach (var property in properties)
			{
				if (!_values.TryGetValue(property, out var storage))
					throw new NoSuchPropertyException(property);

				storage.SetToDefault();
			}
		}

		/// <summary>
		///    Sets all properties in this buffer to their <see cref="IColumnDescriptor.DefaultValue"/>.
		/// </summary>
		public void SetToDefault()
		{
			foreach (var pair in _values)
			{
				pair.Value.SetToDefault();
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
			object Value { get; }
			bool SetValue(object value);
			void CopyFrom(IPropertiesBuffer properties);
			void CopyTo(IPropertiesBuffer destination);
			void SetToDefault();
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
			}

			public bool SetValue(object value)
			{
				if (Equals(value, _value))
					return false;

				_value = (T) value;
				return true;
			}

			public bool SetValue(T value)
			{
				if (Equals(value, _value))
					return false;

				_value = value;
				return true;
			}

			public void CopyFrom(IPropertiesBuffer properties)
			{
				_value = properties.GetValue(_property);
			}

			public void CopyTo(IPropertiesBuffer destination)
			{
				destination.SetValue(_property, _value);
			}

			public void SetToDefault()
			{
				_value = _property.DefaultValue;
			}

			object IPropertyStorage.Value
			{
				get { return Value; }
			}
		}
	}
}