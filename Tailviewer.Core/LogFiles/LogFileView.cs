using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A custom debugger visualizer for <see cref="ILogFile" /> implementations.
	/// </summary>
	public sealed class LogFileView
	{
		private readonly ILogFile _logFile;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logFile"></param>
		public LogFileView(ILogFile logFile)
		{
			_logFile = logFile;
		}

		/// <summary>
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public LogFileProperty[] Properties
		{
			get
			{
				var properties = new List<LogFileProperty>(_logFile.Properties.Count);
				foreach (var property in _logFile.Properties)
					properties.Add(new LogFileProperty(property, _logFile.GetValue(property)));

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
				var count = _logFile.Count;
				var buffer = new LogEntryBuffer(count, _logFile.Columns);
				_logFile.GetEntries(new LogFileSection(0, count), buffer);
				return buffer.ToArray();
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