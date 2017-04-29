using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings.Dashboard.Analysers.Event;

namespace Tailviewer.BusinessLogic.Analysers.Event
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class LogEventDefinition
	{
		public LogEventDefinition(EventSettings settings)
		{
			
		}

		public string TryExtractEventFrom(LogLine logLine)
		{
			throw new System.NotImplementedException();
		}
	}
}