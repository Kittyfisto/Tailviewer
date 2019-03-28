using System;
using System.Runtime.Serialization;

namespace Tailviewer.Analysis.DataSources.BusinessLogic
{
	[DataContract]
	public sealed class DataSourceResult
		: ISerializableType
	{
		public DataSourceId Id;
		public string Name;
		public long? SizeInBytes;
		public DateTime? Created;
		public DateTime? LastModified;

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", Id);
			writer.WriteAttribute("Name", Name);
			writer.WriteAttribute("SizeInBytes", SizeInBytes);
			writer.WriteAttribute("Created", Created);
			writer.WriteAttribute("LastModified", LastModified);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out Id);
			reader.TryReadAttribute("Name", out Name);
			reader.TryReadAttribute("SizeInBytes", out SizeInBytes);
			reader.TryReadAttribute("Created", out Created);
			reader.TryReadAttribute("LastModified", out LastModified);
		}

		public DataSourceResult Clone()
		{
			return new DataSourceResult
			{
				Id = Id,
				Name = Name,
				SizeInBytes = SizeInBytes,
				Created = Created,
				LastModified = LastModified
			};
		}
	}
}