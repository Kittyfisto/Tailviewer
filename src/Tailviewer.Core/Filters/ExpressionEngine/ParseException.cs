using System;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ParseException
		: Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public ParseException()
		{}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public ParseException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public ParseException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}