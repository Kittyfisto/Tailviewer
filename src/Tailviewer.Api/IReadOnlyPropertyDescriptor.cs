using System;
using System.Collections;
using System.Collections.Generic;

namespace Tailviewer
{
	/// <summary>
	///     Describes a particular property of an <see cref="ILogSource"/>.
	/// </summary>
	/// <remarks>
	///     Read-Only properties exist to forward information about the log source *from* the source *to* the user.
	///     A user may not change them, but the source may change the properties' values over time.
	///     Properties with the intention of having the user forward information *to* the log source which implement
	///     <see cref="IPropertyDescriptor"/> instead.
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
		///     The default value of this property which is used by Tailviewer when the property is requested, but hasn't been set before.
		/// </summary>
		object DefaultValue { get; }

		/// <summary>
		///    The comparer Tailviewer should use to compare two values of this property.
		/// </summary>
		IEqualityComparer Comparer { get; }
	}

	/// <summary>
	///     Describes a particular property of type <typeparamref name="T"/> of an <see cref="ILogSource"/>.
	/// </summary>
	/// <remarks>
	///     Read-Only properties exist to forward information about the log source *from* the source *to* the user.
	///     A user may not change them, but the source may change the properties' values over time.
	///     Properties with the intention of having the user forward information *to* the log source which implement
	///     <see cref="IPropertyDescriptor{T}"/> instead.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlyPropertyDescriptor<T>
		: IReadOnlyPropertyDescriptor
	{
		/// <summary>
		///     The default value of this property which is used by Tailviewer when the property is requested, but hasn't been set before.
		/// </summary>
		new T DefaultValue { get; }

		/// <summary>
		///    The comparer Tailviewer should use to compare two values of this property.
		/// </summary>
		new IEqualityComparer<T> Comparer { get; }
	}
}