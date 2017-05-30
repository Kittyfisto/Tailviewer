namespace Tailviewer.BusinessLogic.Analysis.Analysers
{
	/// <summary>
	///     The interface for each and every <see cref="ILogAnalyser" /> implementation.
	///     If the analyser needs to be configured (i.e. there are parameters that need
	///     to be forwarded to the analyser), then this interface should be implemented
	///     to hold the actual configuration.
	/// </summary>
	public interface ILogAnalyserConfiguration
	{
	}
}