using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer
{
	/// <summary>
	///     Thrown when a column which doesn't exist is accessed.
	/// </summary>
	public class NoSuchColumnException
		: ArgumentException
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="column"></param>
		public NoSuchColumnException(ILogFileColumnDescriptor column)
			: base(string.Format("No such column: '{0}'", column.Id))
		{}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		public NoSuchColumnException(string message)
			: base(message)
		{ }
	}
}