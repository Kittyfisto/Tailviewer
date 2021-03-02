using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.Core.Properties
{
	/// <summary>
	///    Provides extension methods to the <see cref="IPropertiesBuffer"/> interface.
	/// </summary>
	public static class LogFilePropertiesExtensions
	{
		/// <summary>
		///     Creates a new view onto the given <paramref name="that" /> which contains only those properties
		///     in the given <paramref name="hiddenProperties" /> list.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="hiddenProperties"></param>
		/// <returns></returns>
		public static IPropertiesBuffer Except(this IPropertiesBuffer that,
		                                       params IReadOnlyPropertyDescriptor[] hiddenProperties)
		{
			return that.Except((IReadOnlyList<IReadOnlyPropertyDescriptor>)hiddenProperties);
		}

		/// <summary>
		///     Creates a new view onto the given <paramref name="that" /> which contains only those properties
		///     in the given <paramref name="hiddenProperties" /> list.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="hiddenProperties"></param>
		/// <returns></returns>
		public static IPropertiesBuffer Except(this IPropertiesBuffer that,
		                                       IReadOnlyList<IReadOnlyPropertyDescriptor> hiddenProperties)
		{
			return new PropertiesBufferHidingView(that, hiddenProperties);
		}
	}
}