namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal interface IInterval<out T>
	{
		T Minimum { get; }
		T Maximum { get; }
	}
}