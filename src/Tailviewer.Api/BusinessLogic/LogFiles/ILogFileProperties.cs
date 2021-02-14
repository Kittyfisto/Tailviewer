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
		/// Reads all values from the given properties object and copies it to this object.
		/// </summary>
		/// <param name="properties"></param>
		void CopyFrom(ILogFileProperties properties);

		/// <summary>
		///     Sets the value of the given property.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchPropertyException"></exception>
		void SetValue(ILogFilePropertyDescriptor property, object value);

		/// <summary>
		///     Sets the value of the given property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchPropertyException"></exception>
		void SetValue<T>(ILogFilePropertyDescriptor<T> property, T value);

		/// <summary>
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue(ILogFilePropertyDescriptor property, out object value);

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue<T>(ILogFilePropertyDescriptor<T> property, out T value);

		/// <summary>
		///     Retrieves the value of the given property.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchPropertyException"></exception>
		object GetValue(ILogFilePropertyDescriptor property);

		/// <summary>
		///     Retrieves the value of the given property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchPropertyException"></exception>
		T GetValue<T>(ILogFilePropertyDescriptor<T> property);

		/// <summary>
		///     Retrieves all values from all properties of this log file and stores them in the given buffer.
		/// </summary>
		/// <remarks>
		///     Values which are not already stored are added to the given buffer.
		/// </remarks>
		/// <param name="destination"></param>
		void CopyAllValuesTo(ILogFileProperties destination);
	}
}