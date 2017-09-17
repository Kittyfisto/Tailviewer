using System.Windows;

namespace Tailviewer
{
	/// <summary>
	///     Extension methods for the <see cref="FrameworkElement" /> class.
	/// </summary>
	/// <remarks>
	///     TODO: Move this class to the Metrolib project
	/// </remarks>
	public static class FrameworkElementExtensions
	{
		/// <summary>
		///     Finds the first ancestor element that has the given name, starting
		///     with the given element.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name"></param>
		/// <returns>The first ancestor with the given name or null if there is none</returns>
		public static FrameworkElement FindFirstAncestorWithName(this FrameworkElement element, string name)
		{
			while (element != null)
			{
				if (element.Name == name)
					return element;

				element = element.Parent as FrameworkElement;
			}

			return null;
		}
	}
}