using System.IO;
using System.Text;

namespace Tailviewer.Test
{
	public static class SerializableTypeExtensions
	{
		private static string Format<T>(MemoryStream stream) where T : class, ISerializableType
		{
			using (var tmp = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
			{
				return tmp.ReadToEnd();
			}
		}
	}
}