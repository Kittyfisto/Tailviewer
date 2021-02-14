using System;
using System.Text;
using System.Threading;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.IO
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class IoScheduler
		: IIoScheduler
	{
		private readonly ITaskScheduler _taskScheduler;

		public IoScheduler(ITaskScheduler taskScheduler)
		{
			_taskScheduler = taskScheduler;
		}

		#region Implementation of IDisposable

		/// <inheritdoc />
		public void Dispose()
		{}

		/// <inheritdoc />
		public ITextFileReader OpenReadText(string fileName,
		                                    ITextFileListener listener,
		                                    Encoding defaultEncoding,
		                                    ILogFileFormatMatcher formatMatcher)
		{
			return new TextFileReader(_taskScheduler, formatMatcher, fileName, defaultEncoding, listener);
		}

		#endregion
	}
}