namespace Tailviewer
{
	/// <summary>
	///     Responsible for creating serializable types from their type name.
	/// </summary>
	public interface ITypeFactory
	{
		/// <summary>
		///     Tries to create a new object of the given type or returns null
		///     if the type is not known or creation failed.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		ISerializableType TryCreateNew(string typeName);
	}
}