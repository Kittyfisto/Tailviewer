using System.Collections.Generic;
using System.Diagnostics;
using Tailviewer.Api;

namespace Tailviewer.Core.Properties
{
	/// <summary>
	///     Provides a custom debugger visualization for an <see cref="IPropertiesBuffer" /> object.
	/// </summary>
	public sealed class PropertiesBufferDebuggerVisualization
	{
		private readonly IPropertiesBuffer _properties;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="properties"></param>
		public PropertiesBufferDebuggerVisualization(IPropertiesBuffer properties)
		{
			_properties = properties;
		}

		/// <summary>
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Dictionary<IReadOnlyPropertyDescriptor, object> Items
		{
			get
			{
				var properties = new Dictionary<IReadOnlyPropertyDescriptor, object>(_properties.Properties.Count);
				foreach (var property in _properties.Properties)
					properties.Add(property, _properties.GetValue(property));

				return properties;
			}
		}
	}
}