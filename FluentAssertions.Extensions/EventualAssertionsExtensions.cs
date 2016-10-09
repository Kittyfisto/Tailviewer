using System;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace FluentAssertions
{
	public static class EventualAssertionsExtensions
	{
		/// <summary>
		///     The amount of time any of the BeXYZ methods block before failing, if no custom
		///     maximumWaitTime has been specified.
		/// </summary>
		public static readonly TimeSpan DefaultWaitTime = TimeSpan.FromSeconds(10);

		public static void BeTrue(this EventualAssertions<bool> that, string message = null)
		{
			BeTrue(that, DefaultWaitTime, message);
		}

		public static void BeTrue(this EventualAssertions<bool> that, TimeSpan maximumWaitTime, string message = null)
		{
			that.Be(true, maximumWaitTime, message);
		}

		public static void BeFalse(this EventualAssertions<bool> that, string message = null)
		{
			BeFalse(that, DefaultWaitTime, message);
		}

		public static void BeFalse(this EventualAssertions<bool> that, TimeSpan maximumWaitTime, string message = null)
		{
			that.Be(false, maximumWaitTime, message);
		}

		public static void Be<T>(this EventualAssertions<T> that, T expectedValue, string message = null)
		{
			Be(that, expectedValue, DefaultWaitTime, message);
		}

		public static void Be<T>(this EventualAssertions<T> that, T expectedValue, TimeSpan maximumWaitTime,
		                         string message = null)
		{
			T currentValue;
			DateTime started = DateTime.UtcNow;
			do
			{
				currentValue = that.GetValue();
				if (Equals(currentValue, expectedValue))
					return;

				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			} while ((DateTime.UtcNow - started) < maximumWaitTime);

			var completeMessage = new StringBuilder();
			completeMessage.AppendFormat("Expected {0}, found found {1}", expectedValue, currentValue);
			if (!string.IsNullOrEmpty(message))
			{
				completeMessage.AppendFormat(
					message.StartsWith("because", StringComparison.InvariantCultureIgnoreCase)
						? ", {0}"
						: ", because {0}"
					, message);
			}

			throw new AssertionException(completeMessage.ToString());
		}
	}
}