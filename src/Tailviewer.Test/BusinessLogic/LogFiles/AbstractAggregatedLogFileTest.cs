using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	/// <summary>
	///     Verifies that an <see cref="ILogFile"/> implementation which aggregates one or more other <see cref="ILogFile"/>s behaves correctly.
	/// </summary>
	[TestFixture]
	public abstract class AbstractAggregatedLogFileTest
	{
		private ManualTaskScheduler _taskScheduler;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
		}

		protected abstract ILogFile Create(ITaskScheduler taskScheduler, ILogFile source);

		private ILogFile Create(ILogFile source)
		{
			var logFile = Create(_taskScheduler, source);
			return logFile;
		}

		#region Well Known Properties

		#region PercentageProcessed

		[Test]
		public void TestPercentageProcessed()
		{
			var source = new Mock<ILogFile>();
			var sourceProperties = new LogFilePropertyList();
			sourceProperties.SetValue(LogFileProperties.PercentageProcessed, Percentage.Zero);
			source.Setup(x => x.Columns).Returns(LogFileColumns.Minimum);
			source.Setup(x => x.GetAllProperties(It.IsAny<ILogFileProperties>()))
			      .Callback((ILogFileProperties destination) => sourceProperties.CopyAllValuesTo(destination));
			source.Setup(x => x.GetProperty(LogFileProperties.PercentageProcessed))
			      .Returns(() => sourceProperties.GetValue(LogFileProperties.PercentageProcessed));
			source.Setup(x => x.Properties).Returns(() => sourceProperties.Properties);

			using (var file = Create(source.Object))
			{
				var fileListener = (ILogFileListener)file;

				file.GetProperty(LogFileProperties.PercentageProcessed).Should().Be(Percentage.Zero, "because the filtered log file hasn't consumed anything of its source (yet)");

				_taskScheduler.RunOnce();
				file.GetProperty(LogFileProperties.PercentageProcessed).Should().Be(Percentage.Zero, "because even though the filter doesn't have anything to do just yet - it's because its own source hasn't even started");

				sourceProperties.SetValue(LogFileProperties.PercentageProcessed, Percentage.FromPercent(42));
				fileListener.OnLogFileModified(source.Object, new LogFileSection(0, 84));
				_taskScheduler.RunOnce();
				file.GetProperty(LogFileProperties.PercentageProcessed).Should().Be(Percentage.FromPercent(42), "because now the filtered log file has processed 100% of the data the source sent it, but the original data source is still only at 42%");

				sourceProperties.SetValue(LogFileProperties.PercentageProcessed, Percentage.HundredPercent);
				fileListener.OnLogFileModified(source.Object, new LogFileSection(84, 200));
				_taskScheduler.RunOnce();
				file.GetProperty(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
			}
		}

		#endregion

		#endregion

	}
}