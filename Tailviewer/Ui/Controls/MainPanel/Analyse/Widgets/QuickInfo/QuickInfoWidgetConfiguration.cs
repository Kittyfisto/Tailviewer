using System.Linq;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.QuickInfo
{
	public sealed class QuickInfoWidgetConfiguration
		: IWidgetConfiguration
	{
		/// <summary>
		///     The title of the individual quick infos.
		/// </summary>
		/// <remarks>
		///     Is part of the view configuration because the title may be changed without analyzing again...
		/// </remarks>
		public QuickInfoTitle[] Titles;

		public object Clone()
		{
			return new QuickInfoWidgetConfiguration
			{
				Titles = Titles?.Select(x => x.Clone()).ToArray()
			};
		}
	}
}