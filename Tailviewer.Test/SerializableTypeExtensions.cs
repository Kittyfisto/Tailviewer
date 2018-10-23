using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Test
{
	public static class SerializableTypeExtensions
	{
		public static T Roundtrip<T>(this T that, params Type[] additionalTypes) where T : class, ISerializableType
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new Writer(stream))
				{
					writer.WriteAttribute("Test", that);
				}

				stream.Position = 0;
				var content = Format<T>(stream);
				TestContext.Progress.WriteLine(content);

				var types = new Dictionary<string, Type>();
				types.Add(typeof(T).FullName, typeof(T));
				foreach (var additional in additionalTypes)
				{
					if (!types.ContainsKey(additional.FullName))
						types.Add(additional.FullName, additional);
				}

				stream.Position = 0;
				var reader = new Reader(stream, new TypeFactory(types));

				T actualValue;
				reader.TryReadAttribute("Test", out actualValue).Should().BeTrue("because an object of type '{0}' should be deserializable",
				                                                                 that.GetType().FullName);
				return actualValue;
			}
		}

		private static string Format<T>(MemoryStream stream) where T : class, ISerializableType
		{
			using (var tmp = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
			{
				return tmp.ReadToEnd();
			}
		}
	}
}