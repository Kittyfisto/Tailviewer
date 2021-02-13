namespace Tailviewer
{
	/// <summary>
	///     The interface for a serializable type.
	/// </summary>
	public interface ISerializableType
	{
		/// <summary>
		///     Serializes this object using the given writer.
		/// </summary>
		/// <param name="writer"></param>
		void Serialize(IWriter writer);

		/// <summary>
		///     Deserializes a previously serialized object again.
		///     This method is called after the parameter less constructor
		///     of this type is called.
		/// </summary>
		/// <param name="reader"></param>
		void Deserialize(IReader reader);
	}
}