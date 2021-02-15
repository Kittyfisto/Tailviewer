using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read/write access to a list of <see cref="IReadOnlyPropertyDescriptor" />s and their corresponding values.
	/// </summary>
	public interface ILogFileProperties
	{
		/// <summary>
		///     The list of properties provided.
		/// </summary>
		IReadOnlyList<IReadOnlyPropertyDescriptor> Properties { get; }

		/// <summary>
		/// Reads all values from the given properties object and copies it to this object.
		/// </summary>
		/// <param name="properties"></param>
		void CopyFrom(ILogFileProperties properties);

		/// <summary>
		///     Sets the value of the given property.
		/// </summary>
		/// <remarks>
		///     Adds it if it doesn't exist yet.
		/// </remarks>
		/// <param name="property"></param>
		/// <param name="value"></param>
		void SetValue(IReadOnlyPropertyDescriptor property, object value);

		/// <summary>
		///     Sets the value of the given property.
		/// </summary>
		/// <remarks>
		///     Adds it if it doesn't exist yet.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <param name="value"></param>
		void SetValue<T>(IReadOnlyPropertyDescriptor<T> property, T value);

		/// <summary>
		///    Tries to retrieve the given property. Sets <paramref name="value"/> to either the value of the property
		///    if said property is part of this collection or to the <see cref="IColumnDescriptor.DefaultValue"/> otherwise.
		/// </summary>
		/// <remarks>
		///    This method exists for those cases where it is important to check if a property actually is part of this collection.
		///    For all other cases, you should use <see cref="GetValue"/> which behaves identical.
		/// </remarks>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns>True when this property is part of the storage, false otherwise</returns>
		bool TryGetValue(IReadOnlyPropertyDescriptor property, out object value);

		/// <summary>
		///    Tries to retrieve the given property. Sets <paramref name="value"/> to either the value of the property
		///    if said property is part of this collection or to the <see cref="IColumnDescriptor{T}.DefaultValue"/> otherwise.
		/// </summary>
		/// <remarks>
		///    This method exists for those cases where it is important to check if a property actually is part of this collection.
		///    For all other cases, you should use <see cref="GetValue"/> which behaves identical.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue<T>(IReadOnlyPropertyDescriptor<T> property, out T value);

		/// <summary>
		///     Retrieves the value of the given property.
		/// </summary>
		/// <param name="property"></param>
		/// <returns>The value of the property or its <see cref="IColumnDescriptor.DefaultValue"/> otherwise</returns>
		object GetValue(IReadOnlyPropertyDescriptor property);

		/// <summary>
		///     Retrieves the value of the given property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <returns>The value of the property or its <see cref="IColumnDescriptor{T}.DefaultValue"/> otherwise</returns>
		T GetValue<T>(IReadOnlyPropertyDescriptor<T> property);

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