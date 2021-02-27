using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tailviewer.Collections
{
	public sealed class ObservableCollectionExt<T>
		: ObservableCollection<T>
			, IObservableCollection<T>
	{
		public ObservableCollectionExt()
		{
		}

		public ObservableCollectionExt(IEnumerable<T> values)
			: base(values)
		{
		}
	}
}