using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Tailviewer
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// TODO: Move this class to the Metrolib project
	/// </remarks>
	public static class DependencyObjectExtensions
	{
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent)
			where T : DependencyObject
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);

				var childType = child as T;
				if (childType != null)
				{
					yield return childType;
				}

				foreach (var other in FindVisualChildren<T>(child))
				{
					yield return other;
				}
			}
		}
	}
}