using System;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class ExportException : Exception
	{
		public ExportException(string message)
			: base(message)
		{ }

		public ExportException(string message, Exception inner)
			: base(message, inner)
		{ }
	}
}