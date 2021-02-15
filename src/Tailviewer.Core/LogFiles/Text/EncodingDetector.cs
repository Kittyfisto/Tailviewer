using System;
using System.IO;
using System.Text;

namespace Tailviewer.Core.LogFiles.Text
{
	internal sealed class EncodingDetector
	{
		private readonly Encoding[] _encodings;


		public EncodingDetector(Encoding[] encodings)
		{
			_encodings = encodings;
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

		public Encoding TryFindEncoding(FileStream stream)
		{
			stream.Position = 0;
			byte[] buffer = new byte[5];
			stream.Read(buffer, 0, 5);

			if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
				return Encoding.UTF8;

			if (buffer[0] == 0xfe && buffer[1] == 0xff)
				return Encoding.Unicode;

			if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
				return Encoding.UTF32;

			if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
				return Encoding.UTF7;

			return null;
		}
	}
}