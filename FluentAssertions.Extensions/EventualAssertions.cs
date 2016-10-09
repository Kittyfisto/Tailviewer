using System;

namespace FluentAssertions
{
	public sealed class EventualAssertions<TProperty>
	{
		private readonly Func<TProperty> _getter;

		public EventualAssertions(Func<TProperty> getter)
		{
			_getter = getter;
		}

		public TProperty GetValue()
		{
			return _getter();
		}
	}
}