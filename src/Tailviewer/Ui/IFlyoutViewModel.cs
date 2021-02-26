using System.ComponentModel;

namespace Tailviewer.Ui
{
	/// <summary>
	/// The view model for a fly-out which is laid on top of other content.
	/// </summary>
	public interface IFlyoutViewModel
		: INotifyPropertyChanged
	{
		void Update();
		string Name { get; }
	}
}