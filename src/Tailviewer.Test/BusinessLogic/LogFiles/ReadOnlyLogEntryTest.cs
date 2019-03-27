using System.Collections.Generic;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class ReadOnlyLogEntryTest
		: AbstractReadOnlyLogEntryTest
	{
		protected override IReadOnlyLogEntry CreateEmpty()
		{
			return new ReadOnlyLogEntry(new Dictionary<ILogFileColumn, object>());
		}
	}
}