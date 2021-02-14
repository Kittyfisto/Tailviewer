using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.IO;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	/// <summary>
	///   Highly inefficient <see cref="ITextFileReader"/> implementation which is only used for testing.
	/// </summary>
	public sealed class InMemoryTextFileReader
		: ITextFileReader
	{
		private readonly ITaskScheduler _taskScheduler;
		private readonly IFilesystem _fileSystem;
		private readonly ITextFileListener _listener;
		private readonly string _fileName;
		private readonly Encoding _defaultEncoding;
		private IPeriodicTask _task;

		public InMemoryTextFileReader(ITaskScheduler taskScheduler,
		                              IFilesystem fileSystem,
		                              ITextFileListener listener,
		                              string fileName,
		                              Encoding defaultEncoding)
		{
			_taskScheduler = taskScheduler;
			_fileSystem = fileSystem;
			_listener = listener;
			_fileName = fileName;
			_defaultEncoding = defaultEncoding;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_taskScheduler.StopPeriodic(_task);
		}

		#endregion

		#region Implementation of ITextFileReader

		public void Start()
		{
			if (_task != null)
				return;

			_task = _taskScheduler.StartPeriodic(RunOnce, TimeSpan.FromSeconds(1));
		}

		private void RunOnce()
		{
			try
			{
				using (var stream = _fileSystem.OpenRead(_fileName))
				using (var reader = new StreamReader(stream, _defaultEncoding))
				{
					
				}
			}
			catch (Exception)
			{
				
			}
		}

		public int Read(LogFileSection section, string[] buffer, int index)
		{
			throw new System.NotImplementedException();
		}

		public int Read(IReadOnlyList<LogLineIndex> section, string[] buffer, int index)
		{
			throw new System.NotImplementedException();
		}

		public Task<int> ReadAsync(LogFileSection section, string[] buffer, int index)
		{
			throw new System.NotImplementedException();
		}

		public Task<int> ReadAsync(IReadOnlyList<LogLineIndex> section, string[] buffer, int index)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}