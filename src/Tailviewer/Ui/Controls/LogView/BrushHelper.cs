using System.Diagnostics.Contracts;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.LogView
{
	public struct BrushHelper
	{
		public readonly Brush NormalBrush;
		public readonly Brush AlternateBrush;

		[Pure]
		public Brush GetBrushFor(int logEntryIndex)
		{
			if (logEntryIndex % 2 != 0)
				return AlternateBrush;

			return NormalBrush;
		}

		public BrushHelper(Color? normal, Color? alternate)
		{
			if (normal != null)
			{
				NormalBrush = new SolidColorBrush(normal.Value);
				NormalBrush.Freeze();
			}
			else
			{
				NormalBrush = null;
			}

			if (alternate != null)
			{
				AlternateBrush = new SolidColorBrush(alternate.Value);
				AlternateBrush.Freeze();
			}
			else
			{
				AlternateBrush = null;
			}
		}
	}
}