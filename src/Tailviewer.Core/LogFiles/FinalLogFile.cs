using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///    This <see cref="ILogFile"/> implementation aggregates most other implementations to:
	///    - Have a single log file implementation for everything (previously, this business logic was part of an IDataSource, but we don't want that anymore)
	///    - Reloading a log file from scratch again
	///      - When the encoding has been changed by the user
	///      - When a plugin has been added / removed
	///      - When the format detector changes its mind and is now even more sure that the format is actually is
	///    - Share functionality between different formats (streaming text files into memory shouldn't be done by the same class which is trying to make sense of the data)
	/// </summary>
	/// <remarks>
	///     TODO: Find better name
	/// </remarks>
	internal sealed class FinalLogFile
		: AbstractLogFile
	{
		public FinalLogFile(ITaskScheduler scheduler) : base(scheduler)
		{
		}

		#region Overrides of AbstractLogFile

		public override IReadOnlyList<IColumnDescriptor> Columns
		{
			get { throw new NotImplementedException(); }
		}

		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { throw new NotImplementedException(); }
		}

		public override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			throw new NotImplementedException();
		}

		public override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			throw new NotImplementedException();
		}

		public override void SetProperty(IPropertyDescriptor property, object value)
		{
			throw new NotImplementedException();
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			throw new NotImplementedException();
		}

		public override void GetAllProperties(ILogFileProperties destination)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                                  IColumnDescriptor<T> column,
		                                  T[] destination,
		                                  int destinationIndex,
		                                  LogFileQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogEntries destination,
		                                int destinationIndex,
		                                LogFileQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
