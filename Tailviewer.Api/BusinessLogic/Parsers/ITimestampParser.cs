using System;

namespace Tailviewer.BusinessLogic.Parsers
{
	public interface ITimestampParser
	{
		bool TryParse(string content, out DateTime timestamp);
	}
}