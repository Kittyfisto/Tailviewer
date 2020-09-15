using System.Text.RegularExpressions;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Serilog
{
	public interface ISerilogMatcher
	{
		string Regex { get; }

		int NumCaptures { get; }

		ILogFileColumn Column { get; }

		void MatchInto(Match match, SerilogEntry logEntry);
	}
}
