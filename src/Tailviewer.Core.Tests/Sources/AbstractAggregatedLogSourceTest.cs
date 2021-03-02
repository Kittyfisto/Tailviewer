using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;

namespace Tailviewer.Core.Tests.Sources
{
	/// <summary>
	///     Verifies that an <see cref="ILogSource"/> implementation which aggregates one or more other <see cref="ILogSource"/>s behaves correctly.
	/// </summary>
	[TestFixture]
	public abstract class AbstractAggregatedLogSourceTest
	{
		private ManualTaskScheduler _taskScheduler;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
		}

		protected abstract ILogSource Create(ITaskScheduler taskScheduler, ILogSource source);

		private ILogSource Create(ILogSource source)
		{
			var logFile = Create(_taskScheduler, source);
			return logFile;
		}
		
		#region Well Known Properties

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/284")]
		[Description("Verifies that aggregated log files pass SetProperty calls down to their source(s)")]
		public void TestSetEncoding()
		{
			var source = new Mock<ILogSource>();
			source.Setup(x => x.Columns).Returns(Core.Columns.Minimum);
			source.Setup(x => x.Properties).Returns(Core.Properties.Minimum);
			using (var file = Create(source.Object))
			{
				file.SetProperty(TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode);
				source.Verify(x => x.SetProperty(TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode), Times.Once);

				file.SetProperty((IPropertyDescriptor)TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode);
				source.Verify(x => x.SetProperty((IPropertyDescriptor)TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode), Times.Once);
			}
		}

		#region PercentageProcessed

		[Test]
		public void TestPercentageProcessed()
		{
			var source = new Mock<ILogSource>();
			var sourceProperties = new PropertiesBufferList();
			sourceProperties.SetValue(Core.Properties.PercentageProcessed, Percentage.Zero);
			source.Setup(x => x.Columns).Returns(Core.Columns.Minimum);
			source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			      .Callback((IPropertiesBuffer destination) => sourceProperties.CopyAllValuesTo(destination));
			source.Setup(x => x.GetProperty(Core.Properties.PercentageProcessed))
			      .Returns(() => sourceProperties.GetValue(Core.Properties.PercentageProcessed));
			source.Setup(x => x.Properties).Returns(() => sourceProperties.Properties);

			using (var file = Create(source.Object))
			{
				var fileListener = (ILogSourceListener)file;

				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.Zero, "because the filtered log file hasn't consumed anything of its source (yet)");

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.Zero, "because even though the filter doesn't have anything to do just yet - it's because its own source hasn't even started");

				sourceProperties.SetValue(Core.Properties.PercentageProcessed, Percentage.FromPercent(42));
				fileListener.OnLogFileModified(source.Object, LogSourceModification.Appended(0, 84));
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.FromPercent(42), "because now the filtered log file has processed 100% of the data the source sent it, but the original data source is still only at 42%");

				sourceProperties.SetValue(Core.Properties.PercentageProcessed, Percentage.HundredPercent);
				fileListener.OnLogFileModified(source.Object, LogSourceModification.Appended(84, 200));
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
			}
		}

		#endregion

		#endregion

	}
}