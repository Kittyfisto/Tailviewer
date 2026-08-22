namespace Tailviewer.Ui.Settings
{
	public sealed class LanguageViewModel
	{
		public LanguageViewModel(string code, string displayName)
		{
			Code = code;
			DisplayName = displayName;
		}

		public string Code { get; }

		public string DisplayName { get; }

		public override string ToString()
		{
			return DisplayName;
		}
	}
}
