using System;

namespace Tailviewer.BusinessLogic.Scheduling
{
	public interface IPeriodicTask
	{
		/// <summary>
		/// 
		/// </summary>
		string Name { get; }

		/// <summary>
		///     Tests if this task should run now.
		/// </summary>
		/// <param name="now"></param>
		/// <param name="remainingWaitTime"></param>
		/// <returns></returns>
		bool ShouldRun(DateTime now, out TimeSpan remainingWaitTime);
	}
}