using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tailviewer.Archiver.Tests
{
	public static class BinaryFormatterExtensions
	{
		public static T Roundtrip<T>(T value)
		{
			using (var stream = new MemoryStream())
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(stream, value);
				stream.Position = 0;

				return (T) bf.Deserialize(stream);
			}
		}
	}
}