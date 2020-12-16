using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.DataSources.UDP
{
	public sealed class UdpLogFile
		: AbstractLogFile
	{
		public UdpLogFile(ITaskScheduler scheduler, string address)
			: base(scheduler)
		{}

		#region Overrides of AbstractLogFile

		public override int MaxCharactersPerLine
		{
			get { throw new NotImplementedException(); }
		}

		public override int Count
		{
			get { throw new NotImplementedException(); }
		}

		public override IReadOnlyList<ILogFileColumn> Columns
		{
			get { throw new NotImplementedException(); }
		}

		public override IReadOnlyList<ILogFilePropertyDescriptor> Properties
		{
			get { throw new NotImplementedException(); }
		}

		public override object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			throw new NotImplementedException();
		}

		public override T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			throw new NotImplementedException();
		}

		public override void GetValues(ILogFileProperties properties)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			throw new NotImplementedException();
		}

		public override LogLine GetLine(int index)
		{
			throw new NotImplementedException();
		}

		public override double Progress
		{
			get { throw new NotImplementedException(); }
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}