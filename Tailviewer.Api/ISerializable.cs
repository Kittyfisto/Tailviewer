using System.Collections.Generic;

namespace Tailviewer
{
	/// <summary>
	///     The interface for a serializable type.
	/// </summary>
	public interface ISerializable
	{
		/// <summary>
		///     Serializes this object using the given writer.
		/// </summary>
		/// <param name="writer"></param>
		void Serialize(IWriter writer);

		/// <summary>
		///     Deserializes a previously serialized object again.
		///     This method is called after the parameterless constructor
		///     of this type is called.
		/// </summary>
		/// <param name="reader"></param>
		void Deserialize(IReader reader);

		#region Deserialization

		//void OnAttributeRead(string name, string value);
		//void OnAttributeRead(string name, int value);
		//void OnAttributeRead(string name, ISerializable value);
		//void OnAttributeRead(string name, IEnumerable<ISerializable> value);

		#endregion
	}
}