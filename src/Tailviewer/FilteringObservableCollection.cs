using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Tailviewer
{
	public sealed class FilteringObservableCollection<T>
		: IEnumerable<T>
		, INotifyCollectionChanged
	{
		private readonly IEnumerable<T> _source;
		private readonly Func<T, bool> _predicate;
		private readonly List<T> _destination;

		public FilteringObservableCollection(IEnumerable<T> source, Func<T, bool> predicate)
		{
			_source = source;
			if (source is INotifyCollectionChanged observable)
				observable.CollectionChanged += SourceOnCollectionChanged;
			_predicate = predicate;
			_destination = new List<T>(source.Where(_predicate));
		}

		private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			throw new NotImplementedException();
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