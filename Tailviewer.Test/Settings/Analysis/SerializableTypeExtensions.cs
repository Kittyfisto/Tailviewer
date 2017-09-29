using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Tailviewer.Core;

namespace Tailviewer.Test.Settings.Analysis
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

				var types = new Dictionary<string, Type>();
				types.Add(typeof(T).FullName, typeof(T));
				foreach (var additional in additionalTypes)
				{
					types.Add(additional.FullName, additional);
				}

				var reader = new Reader(stream, new TypeFactory(types));

				T actualValue;
				reader.TryReadAttribute("Test", out actualValue).Should().BeTrue();
				return actualValue;
			}
		}
	}
}