using System.Runtime.Serialization;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	[DataContract]
	public enum PageLayout
	{
		[EnumMember]
		None = 0,

		[EnumMember]
		WrapHorizontal = 1
	}
}