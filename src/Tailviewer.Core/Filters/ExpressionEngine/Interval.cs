namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class Interval<T>
		: IInterval<T>
	{
		private readonly T _minimum;
		private readonly T _maximum;

		public Interval(T minimum, T maximum)
		{
			_minimum = minimum;
			_maximum = maximum;
		}

		#region Implementation of IInterval<out T>

		public T Minimum => _minimum;

		public T Maximum => _maximum;

		#endregion
	}
}