using System;
using Tailviewer.Api;

namespace Tailviewer.Core.Columns
{
	/// <summary>
	/// </summary>
	public static class ColumnDescriptorExtensions
	{
		/// <summary>
		///     Tests if the given value is compatible with the given column's type.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsAssignableFrom(this IColumnDescriptor column, object value)
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