using System.ComponentModel;

namespace NewDesign.Dashboard.Widgets.QuickInfo
{
	public interface INamedValueViewModel
		: INotifyPropertyChanged
	{
		string Name { get; set; }
		object Value { get; }
	}
}