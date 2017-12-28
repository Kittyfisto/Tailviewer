using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// </summary>
	public static class LogFileColumn
	{
		/// <summary>
		///     Tests if the given value is compatible with the given column's type.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsAssignableFrom(ILogFileColumn column, object value)
		{
			var type = column.DataType;
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