using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Maintains a collection of well-known log file properties only applicable to text-based log files.
	/// </summary>
	public static class TextLogFileProperties
	{
		/// <summary>
		///     The number of lines the original 
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<long> LineCount;

		static TextLogFileProperties()
		{
			LineCount = new WellKnownLogFilePropertyDescriptor<long>(new []{"text", "line_count"}, "Lines");
		}
	}
}