using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core
{
	/// <summary>
	///     Thrown when a column which doesn't exist is accessed.
	/// </summary>
	public sealed class NoSuchColumnException
		: Exception
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="column"></param>
		public NoSuchColumnException(ILogFileColumn column)
			: base(string.Format("There is no column with the id '{0}'", column.Id))
		{
		}
	}
}