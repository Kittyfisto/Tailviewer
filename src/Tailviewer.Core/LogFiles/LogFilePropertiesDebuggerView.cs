using System.Collections.Generic;
using System.Diagnostics;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Provides a custom debugger visualization for a <see cref="ILogFileProperties" /> object.
	/// </summary>
	public sealed class LogFilePropertiesDebuggerView
	{
		private readonly ILogFileProperties _properties;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="properties"></param>
		public LogFilePropertiesDebuggerView(ILogFileProperties properties)
		{
			_properties = properties;
		}

		/// <summary>
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Dictionary<ILogFilePropertyDescriptor, object> Items
		{
			get
			{
				var properties = new Dictionary<ILogFilePropertyDescriptor, object>(_properties.Properties.Count);
				foreach (var property in _properties.Properties)
					properties.Add(property, _properties.GetValue(property));

				return properties;
			}
		}
	}
}