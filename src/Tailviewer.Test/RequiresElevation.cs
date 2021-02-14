using NUnit.Framework;

namespace Tailviewer.Test
{
	/// <summary>
	///     Attribute to mark tests that require elevated rights to run
	/// </summary>
	public sealed class RequiresElevation
		: CategoryAttribute
	{
		public RequiresElevation(string description)
			: base("RequiresElevation")
		{
		}
	}
}