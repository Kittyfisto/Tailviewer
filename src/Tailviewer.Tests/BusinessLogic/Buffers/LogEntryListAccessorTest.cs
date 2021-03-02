using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Tests.BusinessLogic.Entries;

namespace Tailviewer.Tests.BusinessLogic.Buffers
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