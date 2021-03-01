using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Test.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class LogSourceModificationTest
	{
		[Test]
		public void TestAppend()
		{
			var modification = LogSourceModification.Appended(10, 41);
			modification.IsAppended(out var section).Should().BeTrue();
			section.Should().Equal(new LogFileSection(10, 41));
			modification.ToString().Should().Be("Appended [#10, #41]");

			modification.IsRemoved(out var removedSection).Should().BeFalse();
			removedSection.Should().Equal(new LogFileSection());

			modification.IsReset().Should().BeFalse();

			modification.IsPropertiesChanged().Should().BeFalse();
		}

		[Test]
		public void TestEquality()
		{
			var appendModification = LogSourceModification.Appended(10, 41);
			var equalAppendModification = LogSourceModification.Appended(10, 41);
			var anotherAppendModification = LogSourceModification.Appended(9, 41);

			appendModification.Should().Be(equalAppendModification);
			equalAppendModification.Should().Be(appendModification);

			appendModification.Should().NotBe(anotherAppendModification);
			anotherAppendModification.Should().NotBe(equalAppendModification);

			var removedModification = LogSourceModification.Removed(10, 41);
			appendModification.Should().NotBe(removedModification);
			equalAppendModification.Should().NotBe(removedModification);
			anotherAppendModification.Should().NotBe(removedModification);

			var anotherRemovedModification = LogSourceModification.Removed(9, 41);
			removedModification.Should().NotBe(anotherRemovedModification);
			anotherRemovedModification.Should().NotBe(anotherAppendModification);

			appendModification.Should().NotBe(LogSourceModification.Reset());
			appendModification.Should().NotBe(LogSourceModification.PropertiesChanged());

			removedModification.Should().NotBe(LogSourceModification.Reset());
			removedModification.Should().NotBe(LogSourceModification.PropertiesChanged());
		}

		[Test]
		public void TestRemove()
		{
			var modification = LogSourceModification.Removed(9, 22);
			modification.IsRemoved(out var removedSection).Should().BeTrue();
			removedSection.Should().Equal(new LogFileSection(9, 22));
			modification.ToString().Should().Be("Removed [#9, #22]");

			modification.IsAppended(out var appendedSection).Should().BeFalse();
			appendedSection.Should().Equal(new LogFileSection());

			modification.IsReset().Should().BeFalse();

			modification.IsPropertiesChanged().Should().BeFalse();
		}

		[Test]
		public void TestReset()
		{
			var modification = LogSourceModification.Reset();
			modification.IsReset().Should().BeTrue();
			modification.ToString().Should().Be("Reset");

			modification.IsRemoved(out var removedSection).Should().BeFalse();
			removedSection.Should().Equal(new LogFileSection());

			modification.IsAppended(out var appendedSection).Should().BeFalse();
			appendedSection.Should().Equal(new LogFileSection());


			modification.IsPropertiesChanged().Should().BeFalse();
		}

		[Test]
		public void TestPropertiesChanged()
		{
			var modification = LogSourceModification.PropertiesChanged();
			modification.IsPropertiesChanged().Should().BeTrue();
			modification.ToString().Should().Be("Properties Changed");

			modification.IsRemoved(out var removedSection).Should().BeFalse();
			removedSection.Should().Equal(new LogFileSection());

			modification.IsAppended(out var appendedSection).Should().BeFalse();
			appendedSection.Should().Equal(new LogFileSection());

			modification.IsReset().Should().BeFalse();
		}

		[Test]
		public void TestSplitAppendOnePart([Values(0, 1, 99, 100)] int count)
		{
			int maxCount = count + 1;
			var modification = LogSourceModification.Appended(new LogLineIndex(0), count);
			modification.Split(maxCount).Should().Equal(new[]
			{
				modification
			}, "because we append less than the maximum number of lines");
		}

		[Test]
		public void TestSplitAppendTwoParts([Values(0, 1, 99, 100)] int count)
		{
			int maxCount = count + 1;
			var modification = LogSourceModification.Appended(new LogLineIndex(0), count);
			modification.Split(maxCount).Should().Equal(new[]
			{
				modification
			}, "because we append less than the maximum number of lines");
		}

		[Test]
		public void TestSplitAppendThreeParts()
		{
			var modification = LogSourceModification.Appended(new LogLineIndex(101), 99);
			modification.Split(33).Should().Equal(new[]
			{
				LogSourceModification.Appended(101, 33), 
				LogSourceModification.Appended(134, 33), 
				LogSourceModification.Appended(167, 33)
			}, "because we append less than the maximum number of lines");
		}

		[Test]
		public void TestSplitAppendThreePartsPartial()
		{
			var modification = LogSourceModification.Appended(new LogLineIndex(101), 67);
			modification.Split(33).Should().Equal(new[]
			{
				LogSourceModification.Appended(101, 33), 
				LogSourceModification.Appended(134, 33), 
				LogSourceModification.Appended(167, 1)
			}, "because we append less than the maximum number of lines");
		}
	}
}