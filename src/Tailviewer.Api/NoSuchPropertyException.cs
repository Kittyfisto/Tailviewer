using System;

namespace Tailviewer.Api
{
	/// <summary>
	/// This exception is thrown when one tries to change the property of a log file which doesn't even have that property.
	/// </summary>
	public sealed class NoSuchPropertyException
		: ArgumentException
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="property"></param>
		public NoSuchPropertyException(IReadOnlyPropertyDescriptor property)
			: base(string.Format("This log file doesn't have a property '{0}'", property))
		{}
	}
}