using System.Collections.ObjectModel;

namespace Tailviewer.Test.Ui.Controls
{
	public sealed class ObservableCollectionExt<T>
		: ObservableCollection<T>
		, IObservableCollection<T>
	{

	}
}