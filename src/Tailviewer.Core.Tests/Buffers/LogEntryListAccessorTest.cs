using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Tests.Entries;

namespace Tailviewer.Core.Tests.Buffers
{
	[TestFixture]
	public sealed class LogEntryListAccessorTest
		: AbstractReadOnlyLogEntryTest
	{
		protected override IReadOnlyLogEntry CreateDefault()
		{
			// TODO: Swap
			var buffer = new LogBufferList(GeneralColumns.Minimum);
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