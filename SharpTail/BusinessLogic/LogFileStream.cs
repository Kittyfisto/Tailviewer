using System;
using System.IO;
using System.Reflection;
using log4net;

namespace SharpTail.BusinessLogic
{
	public sealed class LogFileStream
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string _fileName;
		private FileStream _source;
		private StreamReader _reader;
		private long _position;

		public LogFileStream(string fileName)
		{
			_fileName = fileName;
		}

		public static LogFileStream OpenRead(string fileName)
		{
			return new LogFileStream(fileName);
		}

		public void Dispose()
		{
			CloseStream();
		}

		public string ReadLine()
		{
			try
			{
				var reader = _reader;
				if (reader != null)
				{
					var line = reader.ReadLine();
					if (line == null)
					{
						CloseStream();
					}

					return line;
				}
			}
			catch (OutOfMemoryException)
			{
				CloseStream();
			}
			catch (IOException e)
			{
				Log.DebugFormat("Caught IO exception: {0}", e);
				CloseStream();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				CloseStream();
			}

			return null;
		}

		private void OpenStream()
		{
			
		}

		private void CloseStream()
		{
			var reader = _reader;
			if (reader != null)
			{
				reader.Dispose();
				_reader = null;
			}

			var source = _source;
			if (source != null)
			{
				source.Dispose();
				_source = null;
			}
		}
	}
}