using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Serilog
{
	public sealed class SerilogFileParserPlugin
		: ITextLogFileParserPlugin
	{
		#region Implementation of ITextLogFileParserPlugin

		public IReadOnlyList<ILogFileFormat> SupportedFormats
		{
			get { throw new NotImplementedException(); }
		}

		public ITextLogFileParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}