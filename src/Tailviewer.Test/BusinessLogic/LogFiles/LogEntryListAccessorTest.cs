using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntryListAccessorTest
		: AbstractReadOnlyLogEntryTest
	{
		protected override IReadOnlyLogEntry CreateDefault()
		{
			// TODO: Swap
			var buffer = new LogBufferList(LogColumns.Minimum);
			buffer.AddEmpty();
			return buffer[0];
		}

		protected override IReadOnlyLogEntry CreateEmpty()
		{
			// TODO: Swap
			var buffer = new LogBufferList();
			buffer.AddEmpty();
			return buffer[0];
		}
	}
}