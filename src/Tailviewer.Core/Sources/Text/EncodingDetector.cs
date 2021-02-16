using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tailviewer.Core.Sources.Text
{
	internal sealed class EncodingDetector
	{
		private readonly List<KeyValuePair<byte[], Encoding>> _encodingsByPreamble;

		public EncodingDetector()
		{
			var encodings = new []
			{
				Encoding.UTF32,
				Encoding.UTF8,
				Encoding.BigEndianUnicode,
				Encoding.Unicode,
			};
			_encodingsByPreamble = encodings.Select(x => new KeyValuePair<byte[], Encoding>(x.GetPreamble(), x))
			                                .Where(x => x.Key != null && x.Key.Length > 0)
			                                .OrderByDescending(x => x.Key.Length)
			                                .ToList();
		}

		public Encoding TryFindEncoding(string fileName)
		{
			try
			{
				using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					return TryFindEncoding(stream);
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		public Encoding TryFindEncoding(Stream stream)
		{
			stream.Position = 0;
			byte[] buffer = new byte[5];
			int bytesRead = stream.Read(buffer, 0, 5);

			foreach (var pair in _encodingsByPreamble)
			{
				if (bytesRead >= pair.Key.Length && ArrayExtensions.StartsWith(buffer, pair.Key))
					return pair.Value;
			}

			return null;
		}
	}
}