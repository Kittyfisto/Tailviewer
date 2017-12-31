using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Describes a particular property of a log file.
	/// </summary>
	public interface ILogFilePropertyDescriptor
	{
		/// <summary>
		///     A unique id which identifies this property amongst all others.
		/// </summary>
		/// <remarks>
		///     There cannot be two different properties with the same id.
		/// </remarks>
		string Id { get; }

		/// <summary>
		/// 
		/// </summary>
		IReadOnlyList<string> Path { get; }

		/// <summary>
		///     The type of the data provided by this column.
		/// </summary>
		Type DataType { get; }

		/// <summary>
		/// 
		/// </summary>
		object DefaultValue { get; }
	}

	/// <summary>
	///     Describes a particular property of type <typeparamref name="T"/> of a log file.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ILogFilePropertyDescriptor<out T>
		: ILogFilePropertyDescriptor
	{
		/// <summary>
		/// 
		/// </summary>
		new T DefaultValue { get; }}
}