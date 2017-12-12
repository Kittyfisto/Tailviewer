using System;

namespace Tailviewer
{
	/// <summary>
	///     An attribute which marks a field, property, method or type which will soon be made
	///     obsolete.
	/// </summary>
	public sealed class WillBeRemoved
		: Attribute
	{
		private readonly string _reason;
		private readonly string _uri;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public WillBeRemoved()
		{
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="uri"></param>
		public WillBeRemoved(string reason, string uri)
		{
			_reason = reason;
			_uri = uri;
		}

		/// <summary>
		/// 
		/// </summary>
		public string Reason => _reason;

		/// <summary>
		/// 
		/// </summary>
		public string Uri => _uri;
	}
}