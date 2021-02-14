using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.IO;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	/// <summary>
	/// Tests <see cref="StreamingTextLogFile"/> together with an actual <see cref="IoScheduler"/>, reading log files from disk.
	/// </summary>
	[TestFixture]
	public sealed class StreamingTextLogFileTest
		: AbstractStreamingTextLogFileTest
	{
		private IoScheduler _ioScheduler;

		[TearDown]
		public void TearDown()
		{
			_ioScheduler?.Dispose();
		}

		#region Overrides of AbstractStreamingTextLogFileTest

		internal override StreamingTextLogFile Create(ServiceContainer serviceContainer, string fileName)
		{
			if (File.Exists(fileName))
				File.Delete(fileName);

			var taskScheduler = serviceContainer.Retrieve<ITaskScheduler>();
			_ioScheduler = new IoScheduler(taskScheduler);
			serviceContainer.RegisterInstance<IIoScheduler>(_ioScheduler);
			return new StreamingTextLogFile(serviceContainer, fileName);
		}

		#endregion
	}
}
