using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///    This class is responsible for detecting the encoding of text file or byte stream.
	/// </summary>
	/// <remarks>
	///    At the moment this class does nothing more than to check if there's a preamble in the file
	///    and to compare it to to a list of well known encodings with preamble.
	/// </remarks>
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

		/// <summary>
		///    Tries to find the encoding of the given file. If the detector isn't sure of the encoding, then null is returned.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
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

		/// <summary>
		///    Tries to find the encoding of the given stream. If the detector isn't sure of the encoding, then null is returned.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
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