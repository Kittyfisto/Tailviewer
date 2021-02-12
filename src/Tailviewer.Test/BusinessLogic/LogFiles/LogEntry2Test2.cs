using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntry2Test2
		: AbstractLogEntryTest
	{
		#region Overrides of AbstractLogEntryTest

		public override ILogEntry CreateDefault()
		{
			return new LogEntry();
		}

		public override ILogEntry CreateEmpty()
		{
			return new LogEntry(new ILogFileColumn[0]);
		}

		public override ILogEntry Create(params ILogFileColumn[] columns)
		{
			return new LogEntry(columns);
		}

		#endregion
	}
}