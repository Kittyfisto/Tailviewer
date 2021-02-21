using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tailviewer.Core.Properties
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerTypeProxy(typeof(PropertiesBufferDebuggerVisualization))]
	internal sealed class PropertiesBufferHidingView
		: IPropertiesBuffer
	{
		private readonly IReadOnlyList<IReadOnlyPropertyDescriptor> _hiddenProperties;
		private readonly IPropertiesBuffer _source;

		/// <summary>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="hiddenProperties"></param>
		public PropertiesBufferHidingView(IPropertiesBuffer source, IReadOnlyList<IReadOnlyPropertyDescriptor> hiddenProperties)
		{
			_source = source;
			_hiddenProperties = hiddenProperties;
		}

		#region Implementation of ILogFileProperties

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { return _source.Properties.Except(_hiddenProperties).ToList(); }
		}

		/// <inheritdoc />
		public void CopyFrom(IPropertiesBuffer properties)
		{
			properties.CopyAllValuesTo(this);
		}

		/// <inheritdoc />
		public bool SetValue(IReadOnlyPropertyDescriptor property, object value)
		{
			if (_hiddenProperties.Contains(property))
				return false;

			return _source.SetValue(property, value);
		}

		/// <inheritdoc />
		public bool SetValue<T>(IReadOnlyPropertyDescriptor<T> property, T value)
		{
			if (_hiddenProperties.Contains(property))
				return false;

			return _source.SetValue(property, value);
		}

		/// <inheritdoc />
		public bool TryGetValue(IReadOnlyPropertyDescriptor property, out object value)
		{
			if (_hiddenProperties.Contains(property))
			{
				value = property.DefaultValue;
				return false;
			}

			return _source.TryGetValue(property, out value);
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(IReadOnlyPropertyDescriptor<T> property, out T value)
		{
			if (_hiddenProperties.Contains(property))
			{
				value = property.DefaultValue;
				return false;
			}

			return _source.TryGetValue(property, out value);
		}

		/// <inheritdoc />
		public object GetValue(IReadOnlyPropertyDescriptor property)
		{
			if (_hiddenProperties.Contains(property))
				return property.DefaultValue;

			return _source.GetValue(property);
		}

		/// <inheritdoc />
		public T GetValue<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			if (_hiddenProperties.Contains(property))
				return property.DefaultValue;

			return _source.GetValue(property);
		}

		/// <inheritdoc />
		public void CopyAllValuesTo(IPropertiesBuffer destination)
		{
			foreach (var property in _source.Properties)
			{
				if (!_hiddenProperties.Contains(property))
					destination.SetValue(property, GetValue(property));
			}
		}

		#endregion
	}
}