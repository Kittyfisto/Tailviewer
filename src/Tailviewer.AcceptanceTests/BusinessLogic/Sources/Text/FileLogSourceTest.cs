using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class FileLogSourceTest
	{
		private ServiceContainer _services;
		private ManualTaskScheduler _taskScheduler;
		private Mock<ILogSourceParserPlugin> _parser;
		private Mock<ILogFileFormatMatcher> _formatMatcher;
		private FileLogSourceFactory _fileLogSourceFactory;

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

			_fileLogSourceFactory = new FileLogSourceFactory(_taskScheduler);
			_formatMatcher = new Mock<ILogFileFormatMatcher>();

			_services.RegisterInstance<IFileLogSourceFactory>(_fileLogSourceFactory);
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ILogSourceParserPlugin>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher.Object);
		}

		private ILogSource CreateEmpty()
		{
			var fileLogSource = new FileLogSource(_services, "", TimeSpan.Zero);
			_taskScheduler.RunOnce();
			return fileLogSource;
		}
	}
}