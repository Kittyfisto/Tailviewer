using System;

namespace Tailviewer
{
	/// <summary>
	///     Describes a particular property of an <see cref="ILogSource"/>.
	/// </summary>
	/// <remarks>
	///     By default, properties are read-only as far as the user is concerned, e.g. properties may change their values over time,
	///     but the user cannot change them back. If this is desired, then the property should implement <see cref="IPropertyDescriptor"/> on top of this one
	///     to mark it as non read-only.
	/// </remarks>
	public interface IReadOnlyPropertyDescriptor
	{
		/// <summary>
		///     A unique id which identifies this property amongst all others.
		/// </summary>
		/// <remarks>
		///     There cannot be two different properties with the same id.
		/// </remarks>
		string Id { get; }

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
	///     Describes a particular property of type <typeparamref name="T"/> of an <see cref="ILogSource"/>.
	/// </summary>
	/// <remarks>
	///     By default, properties are read-only as far as the user is concerned, e.g. properties may change their values over time,
	///     but the user cannot change them back. If this is desired, then the property should implement <see cref="IPropertyDescriptor"/> on top of this one
	///     to mark it as non read-only.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlyPropertyDescriptor<out T>
		: IReadOnlyPropertyDescriptor
	{
		/// <summary>
		/// 
		/// </summary>
		new T DefaultValue { get; }
	}
}