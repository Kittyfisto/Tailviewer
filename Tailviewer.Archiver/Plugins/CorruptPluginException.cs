using System;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     This exception is thrown when a plugin has been corrupted by the dark
	///     side and deals in absolutes.
	/// </summary>
	public sealed class CorruptPluginException
		: Exception
	{
		public CorruptPluginException(string message)
			: base(message)
		{ }
	}
}