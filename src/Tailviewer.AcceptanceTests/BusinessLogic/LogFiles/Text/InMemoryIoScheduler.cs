using System.IO;
using System.Text;
using System.Threading;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.IO;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class InMemoryIoScheduler
		: IIoScheduler
	{
		private readonly ITaskScheduler _taskScheduler;
		private readonly IFilesystem _fileSystem;

		public InMemoryIoScheduler(ITaskScheduler taskScheduler, IFilesystem fileSystem)
		{
			_taskScheduler = taskScheduler;
			_fileSystem = fileSystem;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{}

		public ITextFileReader OpenReadText(string fileName,
		                                    ITextFileListener listener,
		                                    Encoding defaultEncoding,
		                                    ILogFileFormatMatcher formatMatcher)
		{
			return new InMemoryTextFileReader(_taskScheduler, _fileSystem, listener, fileName, defaultEncoding);
		}

		#endregion
	}
}