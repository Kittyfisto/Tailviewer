using System.Collections.Generic;
using System.Diagnostics;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Provides a custom debugger visualization for a <see cref="ILogFileProperties" /> object.
	/// </summary>
	public sealed class LogFilePropertiesView
	{
		private readonly ILogFileProperties _properties;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="properties"></param>
		public LogFilePropertiesView(ILogFileProperties properties)
		{
			_properties = properties;
		}

		/// <summary>
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public LogFileProperty[] Items
		{
			get
			{
				var properties = new List<LogFileProperty>(_properties.Properties.Count);
				foreach (var property in _properties.Properties)
					properties.Add(new LogFileProperty(property, _properties.GetValue(property)));

				return properties.ToArray();
			}
		}

		/// <summary>
		///     Holds the value for a property descriptor.
		/// </summary>
		public struct LogFileProperty
		{
			/// <summary>
			/// </summary>
			public readonly ILogFilePropertyDescriptor PropertyDescriptor;

			/// <summary>
			/// </summary>
			public readonly object Value;

			/// <summary>
			/// </summary>
			/// <param name="property"></param>
			/// <param name="value"></param>
			public LogFileProperty(ILogFilePropertyDescriptor property, object value)
			{
				PropertyDescriptor = property;
				Value = value;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return string.Format("{0}: {1}", PropertyDescriptor?.Id, Value);
			}
		}
	}
}