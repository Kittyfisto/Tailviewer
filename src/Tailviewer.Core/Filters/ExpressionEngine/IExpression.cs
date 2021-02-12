using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	/// <summary>
	/// 
	/// </summary>
	public interface IExpression<out T>
		: IExpression
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		new T Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry);
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IExpression
	{
		/// <summary>
		/// 
		/// </summary>
		Type ResultType { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		object Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry);
	}
}
