using System.ComponentModel;

namespace NewDesign.Dashboard.Widgets.QuickInfos
{
	public interface INamedValueViewModel
		: INotifyPropertyChanged
	{
		string Name { get; }
		object Value { get; }
	}
}