using System.Collections.Generic;

namespace TablePlayground
{
	public sealed class LogResponse
	{
		public IReadOnlyList<LogEntry2> Entries { get; set; }
	}
}