using System;
using System.Collections.Generic;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Parsers;

namespace Tailviewer.Tests
{
	public class SimpleLogEntryParserPlugin
		: ILogEntryParserPlugin
	{
		#region Implementation of ITextLogFileParserPlugin

		public IReadOnlyList<ILogFileFormat> SupportedFormats
		{
			get { throw new NotImplementedException(); }
		}

		public ILogEntryParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			ITimestampParser timestampParser = services.TryRetrieve<ITimestampParser>() ?? new TimestampParser();
			return new GenericTextLogEntryParser(timestampParser);
		}

		#endregion
	}
}