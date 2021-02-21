﻿using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.Core.Sources.Text.Streaming;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class FileLogSourceFactory
		: IFileLogSourceFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="taskScheduler"></param>
		public FileLogSourceFactory(ITaskScheduler taskScheduler)
		{
			_taskScheduler = taskScheduler;
		}

		#region Implementation of IFileLogSourceFactory

		/// <inheritdoc />
		public ILogSource OpenRead(string fileName, ILogFileFormat format, Encoding encoding)
		{
			if (format.IsText)
			{
				//return new TextLogSource(_taskScheduler, fileName, format, encoding);
				return new StreamingTextLogSource(_taskScheduler, fileName, format, encoding);
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