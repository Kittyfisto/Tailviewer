using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.Analysis;

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
		bool TryReadAttribute(string name, out Version value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out DateTime value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out Guid value);

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
		bool TryReadAttribute(string name, out long value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out WidgetId value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out LogAnalyserFactoryId value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out DataSourceId value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out AnalysisId value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out AnalyserId value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <remarks>
		///     Use this overload when reading polymorphic serializable types:
		///     The serializer will create a new instance for you.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out ISerializableType value);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <remarks>
		///     Use this overload when reading polymorphic serializable types:
		///     The serializer will create a new instance for you.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute<T>(string name, out T value) where T : class, ISerializableType;

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute<T>(string name, out T? value) where T : struct, ISerializableType;

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <remarks>
		///     Use this overload when reading non-polymorphic serializable types or
		///     when the type of <see cref="ISerializableType" /> is already known
		///     and created.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryReadAttribute<T>(string name, T value) where T : class, ISerializableType;

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		bool TryReadAttribute(string name, out IEnumerable<ISerializableType> values);

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		bool TryReadAttribute<T>(string name, out IEnumerable<T> values) where T : class, ISerializableType;

		/// <summary>
		///     Tries to read the attribute with the given name.
		///     Returns false if there is no such attribute or some other prevented
		///     reading back the value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values">The list of values: Child elements will be added to it</param>
		/// <returns></returns>
		bool TryReadAttribute<T>(string name, List<T> values) where T : class, ISerializableType;
	}
}