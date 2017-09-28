using System.Collections.Generic;

namespace Tailviewer
{
	/// <summary>
	///     The interface for a writer that allows writing a tree-like structure
	///     in a forward fashion only.
	/// </summary>
	public interface IWriter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, string value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, int value);

		/// <summary>
		///     Use this method to serialize a child owned by your
		///     <see cref="ISerializable" />.
		///     Will in turn call <see cref="ISerializable.Serialize" />.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, ISerializable value);

		/// <summary>
		///     Use this method to serialize a list of children owned by your
		///     <see cref="ISerializable" />.
		///     Will in turn call <see cref="ISerializable.Serialize" /> on each non-null child.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		void WriteAttribute(string name, IEnumerable<ISerializable> values);
	}
}