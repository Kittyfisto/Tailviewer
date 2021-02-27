using System.Collections.Generic;
using System.Collections.Specialized;

namespace Tailviewer.Collections
{
	public interface IObservableCollection<out T>
		: IEnumerable<T>
		, INotifyCollectionChanged
	{

	}
}