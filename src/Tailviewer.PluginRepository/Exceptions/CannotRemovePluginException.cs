using System;

namespace Tailviewer.PluginRepository.Exceptions
{
	public class CannotRemovePluginException
		: Exception
	{
		public CannotRemovePluginException(string message)
			: base(message)
		{ }
	}
}