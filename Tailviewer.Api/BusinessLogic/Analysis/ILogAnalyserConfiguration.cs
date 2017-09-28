using System;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     The interface for each and every <see cref="ILogAnalyser" /> implementation.
	///     If the analyser needs to be configured (i.e. there are parameters that need
	///     to be forwarded to the analyser), then this interface should be implemented
	///     to hold the actual configuration.
	/// </summary>
	public interface ILogAnalyserConfiguration
		: ISerializableType
		, ICloneable
	{
		/// <summary>
		///     It is advisable that the implementation properly implements this method
		///     so that two configurations are equivalent when they will undoubtably produce
		///     the same result for the same data. If there is any doubt that they will,
		///     then the implementation shall return false.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		bool IsEquivalent(ILogAnalyserConfiguration other);
	}
}