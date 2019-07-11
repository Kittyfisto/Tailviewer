namespace Tailviewer.Archiver.Plugins.Description
{
	public sealed class Change
		: IChange
	{
		public Change()
		{}

		public Change(SerializableChange serializableChange)
		{
			Summary = serializableChange.Summary;
			Description = serializableChange.Description;
		}

		public string Summary { get; set; }
		public string Description { get; set; }
	}
}