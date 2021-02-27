using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Tailviewer.Collections
{
	/// <summary>
	///     Projects a (possibly <see cref="INotifyCollectionChanged" />) source into a destination.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TDestination"></typeparam>
	public sealed class ProjectingObservableCollection<TSource, TDestination>
		: IObservableCollection<TDestination>
	{
		private readonly IEnumerable<TSource> _source;
		private readonly List<TDestination> _destination;
		private readonly Func<TSource, TDestination> _projector;

		public ProjectingObservableCollection(IEnumerable<TSource> source, Func<TSource, TDestination> projector)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
			if (source is INotifyCollectionChanged observable)
				observable.CollectionChanged += SourceOnCollectionChanged;
			_projector = projector ?? throw new ArgumentNullException(nameof(projector));
			_destination = new List<TDestination>(_source.Select(x => projector(x)));
		}

		private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
				case NotifyCollectionChangedAction.Add:
					Add(e);
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(e);
					break;
				case NotifyCollectionChangedAction.Replace:
					break;
			}
		}

		private void Remove(NotifyCollectionChangedEventArgs args)
		{
			var oldItems = new TDestination[args.OldItems.Count];
			_destination.CopyTo(args.OldStartingIndex, oldItems, 0, oldItems.Length);
			_destination.RemoveRange(args.OldStartingIndex, oldItems.Length);
			EmitCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, args.OldStartingIndex));
		}

		private void Add(NotifyCollectionChangedEventArgs args)
		{
			var newItems = new List<TDestination>(args.NewItems.Count);
			for (int i = 0; i < args.NewItems.Count; ++i)
			{
				var newItem = _projector((TSource) args.NewItems[i]);
				newItems.Add(newItem);
				_destination.Insert(args.NewStartingIndex + i, newItem);
			}
			EmitCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, args.NewStartingIndex));
		}

		private void Reset()
		{
			_destination.Clear();
			EmitCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#region Implementation of INotifyCollectionChanged

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region Implementation of IEnumerable

		public IEnumerator<TDestination> GetEnumerator()
		{
			return _destination.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of IReadOnlyCollection<out TDestination>

		public int Count
		{
			get { return _destination.Count; }
		}

		#endregion

		#region Implementation of IReadOnlyList<out TDestination>

		public TDestination this[int index]
		{
			get
			{
				return _destination[index];
			}
		}

		#endregion

		private void EmitCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}
	}
}