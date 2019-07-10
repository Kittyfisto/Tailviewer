using System;

namespace Tailviewer.PluginRepository.Exceptions
{
	public sealed class CannotAddUserException
		: Exception
	{
		public CannotAddUserException(string message)
			: base(message)
		{ }
	}
}