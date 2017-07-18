using System;

namespace Tailviewer.Core
{
	public interface ITimestampParser
	{
		bool TryParse(string content, out DateTime timestamp);
	}
}