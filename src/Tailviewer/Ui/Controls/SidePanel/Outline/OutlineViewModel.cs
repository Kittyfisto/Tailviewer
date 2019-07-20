using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.SidePanel.Synopsis
{
	public sealed class OutlineViewModel
		: AbstractSidePanelViewModel
	{
		public OutlineViewModel()
		{
			Tooltip = "Show outline of the current log file";
		}

		#region Overrides of AbstractSidePanelViewModel

		public override Geometry Icon
		{
			get { return Icons.FileDocumentOutline; }
		}

		public override string Id
		{
			get { return "synopsis"; }
		}

		public override void Update()
		{
			
		}

		#endregion
	}
}
