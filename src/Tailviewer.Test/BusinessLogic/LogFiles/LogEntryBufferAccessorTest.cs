using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntryBufferAccessorTest
		: AbstractReadOnlyLogEntryTest
	{
		protected override IReadOnlyLogEntry CreateDefault()
		{
			// TODO: Swap
			var buffer = new LogBufferArray(1, LogColumns.Minimum);
			return buffer[0];
		}

		protected override IReadOnlyLogEntry CreateEmpty()
		{
			// TODO: Swap
			var buffer = new LogBufferArray(1);
			return buffer[0];
		}
	}
}