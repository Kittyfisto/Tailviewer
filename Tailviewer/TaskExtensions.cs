using System;
using System.Threading.Tasks;

namespace Tailviewer
{
	public static class TaskExtensions
	{
		/// <summary>
		///     Blocks until the result of the given task is available.
		///     Unpacks <see cref="AggregateException" />s and throws the first inner exception.
		///     Rethrows the <see cref="AggregateException" /> if it doesn't have any inner exceptions.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <returns></returns>
		public static T AwaitResult<T>(this Task<T> that)
		{
			try
			{
				return that.Result;
			}
			catch (AggregateException e)
			{
				var ex = e.InnerExceptions;
				if (ex.Count > 0)
				{
					var inner = ex[index: 0];
					if (inner != null)
						throw inner;
				}

				throw;
			}
		}
	}
}