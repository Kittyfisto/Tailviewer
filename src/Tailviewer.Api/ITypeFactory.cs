using System;

namespace Tailviewer.Api
{
	/// <summary>
	///     Responsible for creating serializable types from their type name.
	/// </summary>
	[Service]
	public interface ITypeFactory
	{
		/// <summary>
		///     Tries to create a new object of the given type or returns null
		///     if the type is not known or creation failed.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		ISerializableType TryCreateNew(string typeName);

		/// <summary>
		///     Tries to lookup the name of the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		string TryGetTypeName(Type type);
	}
}