using System.Runtime.Serialization;

namespace Tailviewer.Analysis.DataSources.BusinessLogic
{
	[DataContract]
	public sealed class DataSource
		: ISerializableType
	{
		public string Name;

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Name", Name);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Name", out Name);
		}

		public DataSource Clone()
		{
			return new DataSource
			{
				Name = Name
			};
		}
	}
}