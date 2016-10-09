using System;

namespace FluentAssertions
{
	public static class ObjectExtensions
	{
		public static PropertyAssertions<T, TProperty> Property<T, TProperty>(this T that, Func<T, TProperty> getter)
		{
			return new PropertyAssertions<T, TProperty>(that, getter);
		}
	}
}
