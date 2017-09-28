using System.Collections.Generic;

namespace Tailviewer
{
	/// <summary>
	///     The interface for a reader that allows reading a tree-like structure
	///     in a forward fashion only.
	/// </summary>
	public interface IReader
	{
		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out string value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out int value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out ISerializable value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out IEnumerable<ISerializable> values);
	}
}