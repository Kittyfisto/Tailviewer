using System;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Describes a file which has been used as a data source in an analysis.
	/// </summary>
	public sealed class FileDescriptor
		: IDataSourceDescriptor
	{
		private DataSourceId _id;
		private string _path;
		private DateTime _lastWriteTime;
		private long _lineCount;
		private long _sizeInBytes;

		/// <summary>
		///     The id of the data source.
		/// </summary>
		public DataSourceId Id
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>
		///     The full path of the file, as it was known at the time this descriptor was created.
		/// </summary>
		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}

		/// <summary>
		///     The timestamp the file was last written to.
		/// </summary>
		public DateTime LastWriteTime
		{
			get { return _lastWriteTime; }
			set { _lastWriteTime = value; }
		}

		/// <summary>
		///     The number of lines of the file at the time this descriptor was created.
		/// </summary>
		public long LineCount
		{
			get { return _lineCount; }
			set { _lineCount = value; }
		}

		/// <summary>
		/// The number of bytes in the file at the time this descriptor was created.
		/// </summary>
		public long SizeInBytes
		{
			get { return _sizeInBytes; }
			set { _sizeInBytes = value; }
		}

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("Path", _path);
			writer.WriteAttribute("LastWriteTime", _lastWriteTime);
			writer.WriteAttribute("LineCount", _lineCount);
			writer.WriteAttribute("SizeInBytes", _sizeInBytes);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);
			reader.TryReadAttribute("Path", out _path);
			reader.TryReadAttribute("LastWriteTime", out _lastWriteTime);
			reader.TryReadAttribute("LineCount", out _lineCount);
			reader.TryReadAttribute("SizeInBytes", out _sizeInBytes);
		}
	}

}
