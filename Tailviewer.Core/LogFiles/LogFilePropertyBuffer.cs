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
	public sealed class LogFilePropertyBuffer
		: ILogFileProperties
	{
		private readonly IReadOnlyList<ILogFilePropertyDescriptor> _propertyDescriptors;
		private readonly Dictionary<ILogFilePropertyDescriptor, object> _values;

		/// <summary>
		/// </summary>
		/// <param name="propertiesDescriptor"></param>
		public LogFilePropertyBuffer(params ILogFilePropertyDescriptor[] propertiesDescriptor)
			: this((IEnumerable<ILogFilePropertyDescriptor>)propertiesDescriptor)
		{}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		public LogFilePropertyBuffer(IEnumerable<ILogFilePropertyDescriptor> properties)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));

			_propertyDescriptors = new List<ILogFilePropertyDescriptor>(properties);
			_values = new Dictionary<ILogFilePropertyDescriptor, object>(_propertyDescriptors.Count);
			foreach (var propertyDescriptor in _propertyDescriptors)
			{
				_values.Add(propertyDescriptor, propertyDescriptor.DefaultValue);
			}
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
				throw new NoSuchPropertyException(propertyDescriptor);

			_values[propertyDescriptor] = value;
		}

		/// <inheritdoc />
		public void SetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor, T value)
		{
			SetValue(propertyDescriptor, (object)value);
		}

		/// <inheritdoc />
		public bool TryGetValue(ILogFilePropertyDescriptor propertyDescriptor, out object value)
		{
			if (!_values.TryGetValue(propertyDescriptor, out value))
			{
				value = propertyDescriptor.DefaultValue;
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor, out T value)
		{
			object fuck;
			bool ret = TryGetValue(propertyDescriptor, out fuck);
			value = (T)fuck;
			return ret;
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
				{
					properties.SetValue(propertyDescriptor, value);
				}
			}
		}
	}
}