// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	///     Describes the severity of an issue.
	/// </summary>
	public enum Severity
	{
		/// <summary>
		///     The issue is a minor one.
		/// </summary>
		Minor,

		/// <summary>
		///     The issue is major and possibly requires a user's attention.
		/// </summary>
		Major,

		/// <summary>
		///     The issue is critical and requires a user's attention.
		/// </summary>
		Critical
	}
}