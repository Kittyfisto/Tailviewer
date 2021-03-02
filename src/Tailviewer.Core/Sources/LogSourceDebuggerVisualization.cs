using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     A custom debugger visualizer for any <see cref="ILogSource" /> implementations.
	/// </summary>
	public sealed class LogSourceDebuggerVisualization
	{
		private readonly ILogSource _logSource;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logSource"></param>
		public LogSourceDebuggerVisualization(ILogSource logSource)
		{
			_logSource = logSource;
		}

		/// <summary>
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public LogFileProperty[] Properties
		{
			get
			{
				var properties = new List<LogFileProperty>(_logSource.Properties.Count);
				foreach (var property in _logSource.Properties)
					properties.Add(new LogFileProperty(property, _logSource.GetProperty(property)));

				return properties.ToArray();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public IReadOnlyLogEntry[] LogEntries
		{
			get
			{
				var count = _logSource.GetProperty(GeneralProperties.LogEntryCount);
				var buffer = new LogBufferArray(count, _logSource.Columns);
				_logSource.GetEntries(new LogSourceSection(0, count), buffer);
				return buffer.ToArray<IReadOnlyLogEntry>();
			}
		}

		/// <summary>
		///     Holds the value for a property descriptor.
		/// </summary>
		public struct LogFileProperty
		{
			/// <summary>
			/// </summary>
			public readonly IReadOnlyPropertyDescriptor PropertyDescriptor;

			/// <summary>
			/// </summary>
			public readonly object Value;

			/// <summary>
			/// </summary>
			/// <param name="property"></param>
			/// <param name="value"></param>
			public LogFileProperty(IReadOnlyPropertyDescriptor property, object value)
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