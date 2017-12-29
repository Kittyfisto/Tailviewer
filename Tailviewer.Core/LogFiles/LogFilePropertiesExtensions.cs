using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Extension methods for <see cref="ILogFileProperties" /> objects.
	/// </summary>
	public static class LogFilePropertiesExtensions
	{
		/// <summary>
		///     Retrieves the value of the given property.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchPropertyException"></exception>
		public static object GetValue(this ILogFileProperties that, ILogFilePropertyDescriptor property)
		{
			if (that == null)
				throw new ArgumentNullException(nameof(that));

			object value;
			if (!that.TryGetValue(property, out value))
				throw new NoSuchPropertyException(property);

			return value;
		}

		/// <summary>
		///     Retrieves the value of the given property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchPropertyException"></exception>
		public static T GetValue<T>(this ILogFileProperties that, ILogFilePropertyDescriptor<T> property)
		{
			if (that == null)
				throw new ArgumentNullException(nameof(that));

			T value;
			if (!that.TryGetValue(property, out value))
				throw new NoSuchPropertyException(property);

			return value;
		}
	}
}