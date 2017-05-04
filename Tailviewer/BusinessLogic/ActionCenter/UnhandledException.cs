using System;
using System.Text;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class UnhandledException
		: IBug
	{
		private readonly Exception _exception;

		public UnhandledException(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			_exception = exception;
		}

		public string Title => "Unhandled exception";

		/// <summary>
		/// </summary>
		/// <remarks>
		///     It's worse enough that we've encountered an unhandled exception,
		///     but bothering the user is even worse.
		/// </remarks>
		public bool ForceShow => false;

		public string Details
		{
			get
			{
				var builder = new StringBuilder();
				builder.AppendFormat("{0}: {1}", _exception.GetType().Name, _exception.Message);
				builder.AppendLine();
				builder.Append(_exception.StackTrace);
				return builder.ToString();
			}
		}
	}
}