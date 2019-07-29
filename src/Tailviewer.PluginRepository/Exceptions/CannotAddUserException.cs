using System;

namespace Tailviewer.PluginRepository.Exceptions
{
	public sealed class CannotAddUserException
		: Exception
	{
		public bool IsError { get; }

		public CannotAddUserException(string message, bool isError = true)
			: base(message)
		{
			IsError = isError;
		}
	}
}