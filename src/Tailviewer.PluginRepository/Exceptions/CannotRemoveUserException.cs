using System;

namespace Tailviewer.PluginRepository.Exceptions
{
	public sealed class CannotRemoveUserException
		: Exception
	{
		public CannotRemoveUserException(string message)
			: base(message)
		{ }
	}
}