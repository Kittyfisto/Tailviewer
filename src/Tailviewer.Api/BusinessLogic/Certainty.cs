namespace Tailviewer.BusinessLogic
{
	/// <summary>
	/// 
	/// </summary>
	public enum Certainty
	{
		/// <summary>
		/// We aren't sure what we're dealing with.
		/// </summary>
		None,

		/// <summary>
		/// The guess might be correct, but we can't say for certain.
		/// </summary>
		Uncertain,

		/// <summary>
		/// We're certain and the format won't change anymore.
		/// </summary>
		Sure
	}
}