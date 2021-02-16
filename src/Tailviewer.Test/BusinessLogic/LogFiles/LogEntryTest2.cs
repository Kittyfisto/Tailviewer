using NUnit.Framework;
using Tailviewer.Core.Entries;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntryTest2
		: AbstractLogEntryTest
	{
		#region Overrides of AbstractLogEntryTest

		public override ILogEntry CreateDefault()
		{
			return new LogEntry();
		}

		public override ILogEntry CreateEmpty()
		{
			return new LogEntry(new IColumnDescriptor[0]);
		}

		public override ILogEntry Create(params IColumnDescriptor[] columns)
		{
			return new LogEntry(columns);
		}

		#endregion
	}
}