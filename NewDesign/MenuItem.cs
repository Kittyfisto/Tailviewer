namespace NewDesign
{
	public sealed class MenuItem
	{
		public MenuItem(string icon, string title)
		{
			Icon = icon;
			Title = title;
		}

		public string Icon { get; }

		public string Title { get; }
	}
}