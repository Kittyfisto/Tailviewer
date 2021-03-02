using System.Runtime.InteropServices;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Api.Tests
{
	[TestFixture]
	public sealed class LogEntrySourceIdTest
	{
		[Test]
		[Description("Verifies that the default value is equal to a default constructed value (no ctor arguments)")]
		public void TestDefault()
		{
			var @default = new LogEntrySourceId();
			@default.Should().Be(LogEntrySourceId.Default);
		}

		[Test]
		[Description("Verifies that the invalid value doesn't equal any other possible id")]
		public void TestInvalid()
		{
			var invalid = LogEntrySourceId.Invalid;
			for (int i = 0; i < 254; ++i)
			{
				var value = new LogEntrySourceId((byte) i);
				value.Should().NotBe(invalid);
			}
		}

		[Test]
		public void TestToString1()
		{
			var value = new LogEntrySourceId(0);
			value.ToString().Should().Be("#0");
		}

		[Test]
		public void TestToString2()
		{
			LogEntrySourceId.Default.ToString().Should().Be("#0");
		}

		[Test]
		public void TestToString3()
		{
			LogEntrySourceId.Invalid.ToString().Should().Be("Invalid");
		}

		[Test]
		public void TestEquality1()
		{
			var value = new LogEntrySourceId(128);
			// ReSharper disable once EqualExpressionComparison
			value.Equals(value).Should().BeTrue();
			value.GetHashCode().Should().Be(value.GetHashCode());
		}

		[Test]
		public void TestEquality2()
		{
			var value = new LogEntrySourceId(128);
			value.Equals(null).Should().BeFalse();
		}

		[Test]
		public void TestEquality3()
		{
			var value = new LogEntrySourceId(128);
			// ReSharper disable once SuspiciousTypeConversion.Global
			value.Equals(42).Should().BeFalse();
		}

		[Test]
		public void TestEquality4()
		{
			var value = new LogEntrySourceId(128);
			var equalValue = new LogEntrySourceId(128);
			var otherValue = new LogEntrySourceId(129);

			value.Equals(equalValue).Should().BeTrue();
			equalValue.Equals(value).Should().BeTrue();

			value.GetHashCode().Should().Be(equalValue.GetHashCode());

			value.Equals(otherValue).Should().BeFalse();
			otherValue.Equals(value).Should().BeFalse();

			equalValue.Equals(otherValue).Should().BeFalse();
			otherValue.Equals(equalValue).Should().BeFalse();
		}

		[Test]
		public void TestOperatorEquals()
		{
			var value = new LogEntrySourceId(128);
			var equalValue = new LogEntrySourceId(128);
			var otherValue = new LogEntrySourceId(129);

			(value == equalValue).Should().BeTrue();
			(equalValue == value).Should().BeTrue();

			(value == otherValue).Should().BeFalse();
			(otherValue == value).Should().BeFalse();

			(equalValue == otherValue).Should().BeFalse();
			(otherValue == equalValue).Should().BeFalse();
		}

		[Test]
		public void TestOperatorNotEquals()
		{
			var value = new LogEntrySourceId(128);
			var equalValue = new LogEntrySourceId(128);
			var otherValue = new LogEntrySourceId(129);

			(value != equalValue).Should().BeFalse();
			(equalValue != value).Should().BeFalse();

			(value != otherValue).Should().BeTrue();
			(otherValue != value).Should().BeTrue();

			(equalValue != otherValue).Should().BeTrue();
			(otherValue != equalValue).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that the size of a value isn't padded to four bytes")]
		public unsafe void TestSize()
		{
			var value = new LogEntrySourceId(128);

			const string reason = "because this id is used in other classes/structs and shouldn't be padded to four bytes";
			Marshal.SizeOf(value).Should().Be(1, reason);
			Marshal.SizeOf<LogEntrySourceId>().Should().Be(1, reason);
			sizeof(LogEntrySourceId).Should().Be(1, reason);
		}

		[Test]
		public void TestConvertToInt1()
		{
			((int) LogEntrySourceId.Invalid).Should().Be(-1);
		}

		[Test]
		public void TestConvertToInt2([Values(0, 1, 2, 10, 100, 200, 254)] byte value)
		{
			((int)new LogEntrySourceId(value)).Should().Be(value);
		}
	}
}