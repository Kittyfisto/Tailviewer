using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Tailviewer
{
	public sealed class FilteringObservableCollection<T>
		: IEnumerable<T>
		, INotifyCollectionChanged
	{
		private readonly IEnumerable<T> _source;
		private readonly Predicate<T> _filter;
		private readonly List<T> _destination;

		public FilteringObservableCollection(IEnumerable<T> source, Predicate<T> filter)
		{
			_source = source;
			_filter = filter;
		}

		#region Implementation of IEnumerable

		public IEnumerator<T> GetEnumerator()
		{
			return _destination.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of INotifyCollectionChanged

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}
	}
}