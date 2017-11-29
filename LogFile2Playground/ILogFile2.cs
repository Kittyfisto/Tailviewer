using System.Threading;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public interface ILogFile2
	{
		/// <summary>
		///     Requests the given section of the log file.
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		Task<LogLineResponse> RequestAsync(LogFileSection section);

		/// <summary>
		///     Requests the given section of the log file.
		/// </summary>
		/// <param name="section"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<LogLineResponse> RequestAsync(LogFileSection section, CancellationToken cancellationToken);
	}
}