using System;
using System.Collections.Generic;

namespace Tailviewer.Api
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
		void WriteAttribute(string name, bool value);

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
		void WriteAttribute(string name, Version value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, DateTime value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, DateTime? value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, Guid value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, int value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, long value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, long? value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, DataSourceId value);

		/// <summary>
		///     Use this method to serialize a child owned by your
		///     <see cref="ISerializableType" />.
		///     Will in turn call <see cref="ISerializableType.Serialize" />.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttribute(string name, ISerializableType value);

		/// <summary>
		///     Use this method to serialize a list of children owned by your
		///     <see cref="ISerializableType" />.
		///     Will in turn call <see cref="ISerializableType.Serialize" /> on each non-null child.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		void WriteAttribute(string name, IEnumerable<ISerializableType> values);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void WriteAttributeEnum<T>(string name, T value);
	}
}