using System.Collections.Generic;

namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class EnumerableExtensions<T>
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly IReadOnlyList<T> Empty;

		static EnumerableExtensions()
		{
			Empty = new List<T>();
		}
	}
}