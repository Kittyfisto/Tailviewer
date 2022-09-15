using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.Sources
{
	public sealed class NoThrowLogEntryParser
		: ILogEntryParser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogEntryParser _inner;

		public NoThrowLogEntryParser(ILogEntryParser inner)
		{
			_inner = inner;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			try
			{
				_inner.Dispose();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		#endregion

		#region Implementation of ITextLogFileParser

		public IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry)
		{
			try
			{
				return _inner.Parse(logEntry);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return logEntry;
			}
		}

		public IEnumerable<IColumnDescriptor> Columns
		{
			get
			{
				try
				{
					return _inner.Columns ?? Enumerable.Empty<IColumnDescriptor>();
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception: {0}", e);
					return Enumerable.Empty<IColumnDescriptor>();
				}
			}
		}

		#endregion
	}
}