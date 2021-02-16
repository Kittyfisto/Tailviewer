using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.Core.Properties
{
	/// <summary>
	///    Provides extension methods to the <see cref="IPropertiesBuffer"/> interface.
	/// </summary>
	public static class LogFilePropertiesExtensions
	{
		/// <summary>
		///     Creates a new view onto the given <paramref name="that" /> which contains only those properties
		///     in the given <paramref name="computedProperties" /> list.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="computedProperties"></param>
		/// <returns></returns>
		public static IPropertiesBuffer Except(this IPropertiesBuffer that,
		                                        params IReadOnlyPropertyDescriptor[] computedProperties)
		{
			return that.Except((IReadOnlyList<IReadOnlyPropertyDescriptor>)computedProperties);
		}

		/// <summary>
		///     Creates a new view onto the given <paramref name="that" /> which contains only those properties
		///     in the given <paramref name="computedProperties" /> list.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="computedProperties"></param>
		/// <returns></returns>
		public static IPropertiesBuffer Except(this IPropertiesBuffer that,
		                                        IReadOnlyList<IReadOnlyPropertyDescriptor> computedProperties)
		{
			return new PropertiesBufferView(that, that.Properties.Except(computedProperties).ToList());
		}
	}
}