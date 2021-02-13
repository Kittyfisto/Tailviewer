using System.IO;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.IO;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;
using Tailviewer.Test.BusinessLogic.LogFiles.Text;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	/// <summary>
	/// Tests <see cref="StreamingTextLogFile"/> together with an <see cref="InMemoryIoScheduler"/>, reading log files from memory.
	/// </summary>
	[TestFixture]
	[Ignore("Not implemented yet")]
	public sealed class InMemoryStreamingTextLogFileTest
		: AbstractStreamingTextLogFileTest
	{
		private IFilesystem _fileSystem;

		#region Overrides of AbstractStreamingTextLogFileTest

		internal override StreamingTextLogFile Create(ServiceContainer serviceContainer, string fileName)
		{
			_fileSystem = new InMemoryFilesystem();
			var ioScheduler = new InMemoryIoScheduler(serviceContainer.Retrieve<ITaskScheduler>(), _fileSystem);
			serviceContainer.RegisterInstance<IIoScheduler>(ioScheduler);
			return new StreamingTextLogFile(serviceContainer, fileName);
		}

		#endregion
	}
}
