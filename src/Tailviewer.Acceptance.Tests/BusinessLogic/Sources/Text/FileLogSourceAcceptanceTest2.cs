using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Tests.Sources;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class FileLogSourceAcceptanceTest2
		: AbstractLogSourceTest
	{
		private ServiceContainer _services;
		private DefaultTaskScheduler _taskScheduler;
		private Mock<ILogSourceParserPlugin> _parser;
		private Mock<ILogFileFormatMatcher> _formatMatcher;
		private IRawFileLogSourceFactory _rawFileLogSourceFactory;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_taskScheduler = new DefaultTaskScheduler();
			_parser = new Mock<ILogSourceParserPlugin>();
			_parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			       .Returns((IServiceContainer services, ILogSource source) =>
			       {
				       return new GenericTextLogSource(source, new GenericTextLogEntryParser());
			       });

			_rawFileLogSourceFactory = new RawFileLogSourceFactory(_taskScheduler);
			_formatMatcher = new Mock<ILogFileFormatMatcher>();

			_services.RegisterInstance<IRawFileLogSourceFactory>(_rawFileLogSourceFactory);
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ILogSourceParserPlugin>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher.Object);
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		#region Overrides of AbstractLogFileTest

		[Ignore("")]
		public override void TestDisposeData()
		{ }

		protected override ILogSource CreateEmpty()
		{
			var fileLogSource = new FileLogSource(_services, "");
			fileLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
			return fileLogSource;
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			var fileName = PathEx.GetTempFileName();
			TestContext.Progress.WriteLine("File: {0}", fileName);

			using (var stream = File.OpenWrite(fileName))
			using(var writer = new StreamWriter(stream))
			{
				for(int i = 0; i < content.Count; ++i)
				{
					var logEntry = content[i];
					if(logEntry.TryGetValue(GeneralColumns.Timestamp, out var timestamp) && timestamp != null)
					{
						// Let's write the timestamp in a format everybody recognizes
						writer.Write("{0:yyyy-MM-dd HH:mm:ss.fffffff}", timestamp);
					}
					writer.Write(logEntry.ToString());

					if (i < content.Count - 1)
						writer.WriteLine();
				}
			}

			var fileLogSource = new FileLogSource(_services, fileName);
			fileLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
			return fileLogSource;
		}

		#endregion
	}
}
