using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NewDesign.Dashboard.Widgets.Events
{
	public sealed class EventsViewModel
		: AbstractWidgetViewModel
	{
		private readonly ObservableCollection<EventViewModel> _events;

		public EventsViewModel()
		{
			_events = new ObservableCollection<EventViewModel>();
		}

		public IEnumerable<EventViewModel> Events => _events;
	}
}