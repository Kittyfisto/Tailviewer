using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read/write access to a list of <see cref="ILogFilePropertyDescriptor" />s.
	/// </summary>
	public interface ILogFileProperties
	{
		/// <summary>
		///     The list of properties provided.
		/// </summary>
		IReadOnlyList<ILogFilePropertyDescriptor> Properties { get; }

		/// <summary>
		///     Sets the value of the given property.
		/// </summary>
		/// <param name="propertyDescriptor"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchPropertyException"></exception>
		void SetValue(ILogFilePropertyDescriptor propertyDescriptor, object value);

		/// <summary>
		///     Sets the value of the given property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyDescriptor"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchPropertyException"></exception>
		void SetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor, T value);

		/// <summary>
		/// </summary>
		/// <param name="propertyDescriptor"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue(ILogFilePropertyDescriptor propertyDescriptor, out object value);

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyDescriptor"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor, out T value);

		/// <summary>
		///     Retrieves a subset of properties from this object and stores them in the given <paramref name="properties" />.
		/// </summary>
		/// <remarks>
		///     Only those properties present in the given <paramref name="properties" /> and in this
		///     one are retrieved.
		/// </remarks>
		/// <param name="properties"></param>
		void GetValues(ILogFileProperties properties);
	}
}