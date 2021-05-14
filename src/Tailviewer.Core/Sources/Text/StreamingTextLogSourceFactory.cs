using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class StreamingTextLogSourceFactory
		: IRawFileLogSourceFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;
		private readonly IFilesystem _filesystem;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filesystem"></param>
		/// <param name="taskScheduler"></param>
		public StreamingTextLogSourceFactory(IFilesystem filesystem, ITaskScheduler taskScheduler)
		{
			_filesystem = filesystem;
			_taskScheduler = taskScheduler;
		}

		#region Implementation of IFileLogSourceFactory

		/// <inheritdoc />
		public ILogSource OpenRead(string fileName, ILogFileFormat format, Encoding encoding)
		{
			if (format.IsText)
			{
				//return new TextLogSource(_taskScheduler, fileName, format, encoding);
				return new StreamingTextLogSource(_filesystem, _taskScheduler, fileName, format, encoding);
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