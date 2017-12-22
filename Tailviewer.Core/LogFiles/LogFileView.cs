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
		/// 
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IReadOnlyLogEntry[] Items
		{
			get
			{
				var count = _logFile.Count;
				var buffer = new LogEntryBuffer(count, _logFile.Columns);
				_logFile.GetEntries(new LogFileSection(0, count), buffer);
				return buffer.ToArray();
			}
		}
	}
}