using System.IO;
using System.Text;
using FluentAssertions;
using SharpRemote;

namespace Tailviewer.PluginRepository.Tests
{
	public static class BinarySerializerTest
	{
		public static T Roundtrip<T>(T value)
		{
			var serializer = new BinarySerializer();
			serializer.RegisterType<T>();

			using (var stream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
				{
					serializer.WriteObject(writer, value, null);
				}

				stream.Position = 0;

				using (var reader = new BinaryReader(stream, Encoding.UTF8))
				{
					var actualValue = serializer.ReadObject(reader, null);
					if (value != null)
						actualValue.Should().BeOfType<T>();
					return (T) actualValue;
				}
			}
		}
	}
}