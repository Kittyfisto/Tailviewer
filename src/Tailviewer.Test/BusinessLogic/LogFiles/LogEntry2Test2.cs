using System;
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
			return new LogEntry2();
		}

		public override ILogEntry CreateEmpty()
		{
			return new LogEntry2(new ILogFileColumn[0]);
		}

		public override ILogEntry Create(params ILogFileColumn[] columns)
		{
			return new LogEntry2(columns);
		}

		#endregion
	}
}