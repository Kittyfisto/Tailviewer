namespace NewDesign
{
	public sealed class DataSource
	{
		public DataSource(string icon, string title)
		{
			Icon = icon;
			Title = title;
		}

		public string Icon { get; }

		public string Title { get; }
	}
}