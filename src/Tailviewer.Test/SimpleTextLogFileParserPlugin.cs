using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Parsers;

namespace Tailviewer.Test
{
	public class SimpleTextLogFileParserPlugin
		: ITextLogFileParserPlugin
	{
		#region Implementation of ITextLogFileParserPlugin

		public IReadOnlyList<ILogFileFormat> SupportedFormats
		{
			get { throw new NotImplementedException(); }
		}

		public ITextLogFileParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			ITimestampParser timestampParser = services.TryRetrieve<ITimestampParser>() ?? new TimestampParser();
			return new TextLogFileParser(timestampParser);
		}

		#endregion
	}
}