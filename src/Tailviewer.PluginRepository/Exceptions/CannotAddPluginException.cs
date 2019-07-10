using System;

namespace Tailviewer.PluginRepository.Exceptions
{
	public class CannotAddPluginException
		: Exception
	{
		public CannotAddPluginException(string message)
			: base(message)
		{ }

		public CannotAddPluginException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}
}
