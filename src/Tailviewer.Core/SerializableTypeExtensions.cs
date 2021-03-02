using System;
using System.IO;
using Tailviewer.Api;

namespace Tailviewer.Core
{
	/// <summary>
	/// </summary>
	public static class SerializableTypeExtensions
	{
		/// <summary>
		///     Roundtrips a serializable value in-memory.
		///     This method is only useful to test <see cref="ISerializableType.Serialize" /> and
		///     <see cref="ISerializableType.Deserialize" /> implementations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="serializableTypes"></param>
		/// <returns></returns>
		public static T Roundtrip<T>(this T value, params Type[] serializableTypes) where T : ISerializableType, new()
		{
			var typeFactory = new TypeFactory();
			typeFactory.Add(typeof(T));
			foreach (var type in serializableTypes) typeFactory.Add(type);
			return Roundtrip(value, typeFactory);
		}

		/// <summary>
		///     Roundtrips a serializable value in-memory.
		///     This method is only useful to test <see cref="ISerializableType.Serialize" /> and
		///     <see cref="ISerializableType.Deserialize" /> implementations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="typeFactory"></param>
		/// <returns></returns>
		public static T Roundtrip<T>(this T value, ITypeFactory typeFactory) where T : ISerializableType, new()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new Writer(stream, typeFactory))
				{
					writer.WriteAttribute("Value", value);
				}

				stream.Position = 0;

				var reader = new Reader(stream, typeFactory);
				reader.TryReadAttribute("Value", out T actualValue);
				return actualValue;
			}
		}
	}
}