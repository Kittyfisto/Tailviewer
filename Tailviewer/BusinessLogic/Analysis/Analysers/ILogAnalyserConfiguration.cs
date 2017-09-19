using System;

namespace Tailviewer.BusinessLogic.Analysis.Analysers
{
	/// <summary>
	///     The interface for each and every <see cref="ILogAnalyser" /> implementation.
	///     If the analyser needs to be configured (i.e. there are parameters that need
	///     to be forwarded to the analyser), then this interface should be implemented
	///     to hold the actual configuration.
	/// </summary>
	/// <remarks>
	///     It is advisable that the implementation properly implements <see cref="object.Equals(object)" />
	///     so that two configurations are equal when they are equivalent (i.e. two configurations
	///     being equal mean that they will, given the same data, produce the same result, whereas
	///     if they are not equal, they may not produce the same result).
	/// </remarks>
	public interface ILogAnalyserConfiguration
		: ICloneable
	{
	}
}