using System;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	[Flags]
	public enum DataSourceDropType
	{
		None = 0,
		ArrangeTop = 0x1,
		ArrangeBottom = 0x2,
		Group = 0x4,
	}
}