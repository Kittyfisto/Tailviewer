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

		public static void BeGreaterOrEqual(this EventualAssertions<int> that, int threshold, TimeSpan maximumWaitTime, string message = null)
		{
			int finalValue;
			if (IsTrue(that, value => value >= threshold, maximumWaitTime, out finalValue))
				return;

			var completeMessage = new StringBuilder();
			completeMessage.AppendFormat("Expected {0} to be greater or equal to {1}",
			                             finalValue,
			                             threshold);
			completeMessage.AppendWaitTime(maximumWaitTime);
			completeMessage.AppendMessage(message);

			throw new AssertionException(completeMessage.ToString());
		}

		public static void Be<T>(this EventualAssertions<T> that, T expectedValue, string message = null)
		{
			Be(that, expectedValue, DefaultWaitTime, message);
		}

		public static void Be<T>(this EventualAssertions<T> that, T expectedValue, TimeSpan maximumWaitTime,
		                         string message = null)
		{
			T finalValue;
			if (IsTrue(that, value => Equals(value, expectedValue), maximumWaitTime, out finalValue))
				return;

			var completeMessage = new StringBuilder();
			completeMessage.AppendFormat("Expected {0}, but found found {1}",
			                             expectedValue,
			                             finalValue);
			completeMessage.AppendWaitTime(maximumWaitTime);
			completeMessage.AppendMessage(message);

			throw new AssertionException(completeMessage.ToString());
		}

		private static bool IsTrue<T>(this EventualAssertions<T> that, Predicate<T> predicate, TimeSpan maximumWaitTime, out T finalValue)
		{
			DateTime started = DateTime.UtcNow;
			do
			{
				finalValue = that.GetValue();
				if (predicate(finalValue))
					return true;

				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			} while ((DateTime.UtcNow - started) < maximumWaitTime);

			return false;
		}

		private static void AppendMessage(this StringBuilder builder, string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				builder.AppendFormat(
					message.StartsWith("because", StringComparison.InvariantCultureIgnoreCase)
						? ", {0}"
						: ", because {0}"
					, message);
			}
		}

		private static void AppendWaitTime(this StringBuilder builder, TimeSpan timespan)
		{
			builder.Append(" after waiting ");
			if (timespan > TimeSpan.FromSeconds(1))
			{
				builder.AppendFormat("{0} second(s)", timespan.TotalSeconds);
			}
			else
			{
				builder.AppendFormat("{0} ms", timespan.TotalMilliseconds);
			}
		}
	}
}