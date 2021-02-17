using System;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test.BusinessLogic.Sources;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class FileLogSourceTest
		: AbstractLogSourceTest
	{
		private ServiceContainer _services;
		private ManualTaskScheduler _taskScheduler;
		private Mock<ILogSourceParserPlugin> _parser;
		private Mock<ILogFileFormatMatcher> _formatMatcher;
		private Mock<IFileLogSourceFactory> _fileLogSourceFactory;
		private InMemoryLogSource _logSource;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_taskScheduler = new ManualTaskScheduler();
			_parser = new Mock<ILogSourceParserPlugin>();
			_parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			       .Returns((IServiceContainer services, ILogSource source) =>
			       {
				       return new GenericTextLogSource(source, new GenericTextLogEntryParser());
			       });

			_logSource = new InMemoryLogSource();

			_fileLogSourceFactory = new Mock<IFileLogSourceFactory>();
			_fileLogSourceFactory
				.Setup(x => x.OpenRead(It.IsAny<string>(), It.IsAny<ILogFileFormat>(), It.IsAny<Encoding>()))
				.Returns(_logSource);
			_formatMatcher = new Mock<ILogFileFormatMatcher>();

			_services.RegisterInstance<IFileLogSourceFactory>(_fileLogSourceFactory.Object);
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ILogSourceParserPlugin>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher.Object);
		}

		[Test]
		public void Test()
		{
			var logSource = CreateEmpty();
			_taskScheduler.RunOnce();

			var entries = logSource.GetEntries();
			entries.Should().BeEmpty();

			_logSource.AddEntry("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
			_taskScheduler.RunOnce();
			entries = logSource.GetEntries();
			entries.Should().HaveCount(1);
			entries[0].RawContent.Should()
			          .Be("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
		}

		#region Overrides of AbstractLogFileTest

		protected override ILogSource CreateEmpty()
		{
			var fileLogSource = new FileLogSource(_services, "", TimeSpan.Zero);
			_taskScheduler.RunOnce();
			return fileLogSource;
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			foreach (var entry in content)
			{
				var copied = new LogEntry(entry.Columns);
				copied.CopyFrom(entry);
				if (entry.Contains(GeneralColumns.Timestamp) && !entry.Contains(GeneralColumns.RawContent))
				{
					copied.RawContent = string.Format("{0:yyyy-MM-dd HH:mm:ss.fffffff}", entry.Timestamp);
				}

				_logSource.Add(copied);
			}
			var fileLogSource = new FileLogSource(_services, "", TimeSpan.Zero);
			_taskScheduler.RunOnce();
			return fileLogSource;
		}

		#endregion
	}
}
