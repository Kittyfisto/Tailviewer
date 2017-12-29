using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer
{
	/// <summary>
	///     Is thrown when a property is accessed which is not present.
	/// </summary>
	public sealed class NoSuchPropertyException
		: ArgumentException
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		public NoSuchPropertyException(ILogFilePropertyDescriptor property)
			: base(string.Format("No such property: '{0}'", property.Id))
		{

		}
	}
}