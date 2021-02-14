using System.ComponentModel;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	interface ILogFilePropertyViewModel
		: INotifyPropertyChanged
	{
		string Title { get; }
		object Value { get; }
		void Update(ILogFileProperties properties);
	}
}