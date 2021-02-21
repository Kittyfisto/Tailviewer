using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.Core.Sources.Text.Simple;
using Tailviewer.Core.Sources.Text.Streaming;

namespace Tailviewer.Core.Sources.Text
{
	internal sealed class FileLogSourceFactory
		: IFileLogSourceFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;

		public FileLogSourceFactory(ITaskScheduler taskScheduler)
		{
			_taskScheduler = taskScheduler;
		}

		#region Implementation of IFileLogSourceFactory

		public ILogSource OpenRead(string fileName, ILogFileFormat format, Encoding encoding)
		{
			if (format.IsText)
			{
				return new TextLogSource(_taskScheduler, fileName, encoding);
				//return new StreamingTextLogSource(_taskScheduler, fileName, encoding);
			}
			else
			{
				Log.WarnFormat("Log file {0} has been determined to be a binary file ({1}) - processing binary files is not implemented", fileName, format);
				return null;
			}
		}

		#endregion
	}
}