using System;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.QuickInfo
{
	public sealed class QuickInfoTitle
		: ICloneable
	{
		public Guid Id;
		public string Title;

		public QuickInfoTitle Clone()
		{
			return new QuickInfoTitle
			{
				Id = Id,
				Title = Title
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}