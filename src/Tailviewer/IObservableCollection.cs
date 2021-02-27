using System.Collections.Generic;
using System.Collections.Specialized;

namespace Tailviewer
{
	public interface IObservableCollection<out T>
		: IEnumerable<T>
		, INotifyCollectionChanged
	{

	}
}