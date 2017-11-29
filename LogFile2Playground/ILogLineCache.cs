using System.Threading;
using System.Threading.Tasks;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public interface ILogLineCache
	{
		Size EstimatedSize { get; }
		Task<LogLineResponse> RequestAsync(LogFileSection section, CancellationToken cancellationToken);
	}
}