using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Tests.BusinessLogic.Entries;

namespace Tailviewer.Tests.BusinessLogic.Buffers
{
	[TestFixture]
	public sealed class LogEntryBufferAccessorTest
		: AbstractReadOnlyLogEntryTest
	{
		protected override IReadOnlyLogEntry CreateDefault()
		{
			// TODO: Swap
			var buffer = new LogBufferArray(1, GeneralColumns.Minimum);
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