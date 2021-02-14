using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///    Provides extension methods to the <see cref="ILogFileProperties"/> interface.
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
		public static ILogFileProperties Except(this ILogFileProperties that,
		                                        params ILogFilePropertyDescriptor[] computedProperties)
		{
			return that.Except((IReadOnlyList<ILogFilePropertyDescriptor>)computedProperties);
		}

		/// <summary>
		///     Creates a new view onto the given <paramref name="that" /> which contains only those properties
		///     in the given <paramref name="computedProperties" /> list.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="computedProperties"></param>
		/// <returns></returns>
		public static ILogFileProperties Except(this ILogFileProperties that,
		                                        IReadOnlyList<ILogFilePropertyDescriptor> computedProperties)
		{
			return new LogFilePropertiesView(that, that.Properties.Except(computedProperties).ToList());
		}
	}
}