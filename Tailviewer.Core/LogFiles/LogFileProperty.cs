using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public static class LogFileProperty
	{
		/// <summary>
		///     Tests if the given value is compatible with the given property's type.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsAssignableFrom(ILogFilePropertyDescriptor property, object value)
		{
			var type = property.DataType;
			if (value == null)
			{
				if (type.IsClass || type.IsInterface || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)))
				{
					return true;
				}

				return false;
			}

			var valueType = value.GetType();
			if (type.IsAssignableFrom(valueType))
				return true;

			return false;
		}
	}
}