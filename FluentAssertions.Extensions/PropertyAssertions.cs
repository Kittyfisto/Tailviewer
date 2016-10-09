using System;

namespace FluentAssertions
{
	public sealed class PropertyAssertions<T, TProperty>
	{
		private readonly T _subject;
		private readonly Func<T, TProperty> _getter;

		public PropertyAssertions(T subject, Func<T, TProperty> getter)
		{
			_subject = subject;
			_getter = getter;
		}

		public EventualAssertions<TProperty> ShouldEventually()
		{
			return new EventualAssertions<TProperty>(() => _getter(_subject));
		}
	}
}