using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Tailviewer.Collections
{
	/// <summary>
	///     Provides a view onto a list of enumerations which is the result of concat-ing them into one big list.
	///     Updates itself when any of the source collections change.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class CompoundObservableCollection<T>
		: IObservableCollection<T>
	{
		private readonly bool _hasSeparator;
		private readonly T _separator;
		private readonly List<IEnumerable<T>> _collections;
		private readonly List<T> _values;

		public CompoundObservableCollection(bool hasSeparator, T separator = default)
		{
			_hasSeparator = hasSeparator;
			_separator = separator;
			_collections = new List<IEnumerable<T>>();
			_values = new List<T>();
		}

		public void Add(IEnumerable<T> collection)
		{
			_collections.Add(collection);
			SubscribeTo(collection);
			UpdateValues();
		}

		public void Insert(int index, IEnumerable<T> collection)
		{
			_collections.Insert(index, collection);
			SubscribeTo(collection);
			UpdateValues();
		}

		public void RemoveAt(int index)
		{
			var collection = _collections[index];
			UnsubscribeFrom(collection);
			_collections.RemoveAt(index);
			UpdateValues();
		}

		public int ChildCollectionCount => _collections.Count;

		#region Implementation of INotifyCollectionChanged

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		private void SubscribeTo(IEnumerable<T> collection)
		{
			if (collection is ObservableCollection<T> observable)
				observable.CollectionChanged += OnChildCollectionChanged;
		}

		private void UnsubscribeFrom(IEnumerable<T> collection)
		{
			if (collection is ObservableCollection<T> observable)
				observable.CollectionChanged -= OnChildCollectionChanged;
		}

		private void OnChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateValues();
		}

		private void UpdateValues()
		{
			_values.Clear();
			EmitCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			bool needsSeparator = false;
			foreach (var collection in _collections)
			{
				if (collection != null)
				{
					bool addedAny = false;
					foreach (var value in collection)
					{
						if (needsSeparator && _hasSeparator)
						{
							_values.Add(_separator);
							needsSeparator = false;
						}

						_values.Add(value);
						addedAny = true;
					}

					if (addedAny)
						needsSeparator = true;
				}
			}
			EmitCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _values,
			                                                           startingIndex: 0));
		}

		private void EmitCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		#region Implementation of IEnumerable

		public IEnumerator<T> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}