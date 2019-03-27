using System;

namespace Tailviewer.Archiver
{
	/// <summary>
	///     An exception that occured during packing of an archive.
	///     Messages are descriptive enough to be printed to the user
	///     (omitting callstack and other things).
	/// </summary>
	public sealed class PackException
		: Exception
	{
		public PackException(string message)
			: base(message)
		{
		}

		public PackException(string message, Exception inner)
			: base(message)
		{
		}
	}
}