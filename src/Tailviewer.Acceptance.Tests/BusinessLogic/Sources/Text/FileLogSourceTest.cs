using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;
using Tailviewer.Core;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class FileLogSourceTest
	{
		private ServiceContainer _services;
		private ManualTaskScheduler _taskScheduler;
		private Mock<ILogSourceParserPlugin> _parser;
		private Mock<ILogFileFormatMatcher> _formatMatcher;
		private StreamingTextLogSourceFactory _rawFileLogSourceFactory;
		private Filesystem _filesystem;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_taskScheduler = new ManualTaskScheduler();
			_filesystem = new Filesystem(_taskScheduler);
			_parser = new Mock<ILogSourceParserPlugin>();
			_parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			       .Returns((IServiceContainer services, ILogSource source) =>
			       {
				       return new GenericTextLogSource(source, new GenericTextLogEntryParser());
			       });

			_rawFileLogSourceFactory = new StreamingTextLogSourceFactory(_filesystem, _taskScheduler);
			_formatMatcher = new Mock<ILogFileFormatMatcher>();

			_services.RegisterInstance<IRawFileLogSourceFactory>(_rawFileLogSourceFactory);
			_services.RegisterInstance<IFilesystem>(_filesystem);
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ILogSourceParserPlugin>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher.Object);
		}

		private ILogSource Create(string fileName)
		{
			var fileLogSource = new FileLogSource(_services, fileName, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			return fileLogSource;
		}

		private string GetUniqueNonExistingFileName()
		{
			var fileName = PathEx.GetTempFileName();
			if (File.Exists(fileName))
				File.Delete(fileName);

			TestContext.WriteLine("FileName: {0}", fileName);
			return fileName;
		}

		[Test]
		public void TestFileDoesNotExist()
		{
			var fileName = GetUniqueNonExistingFileName();

			var source = Create(fileName);
			_taskScheduler.Run(5);

			source.GetProperty(Properties.EmptyReason).Should().BeOfType<SourceDoesNotExist>();
			source.GetProperty(Properties.Created).Should().Be(null);
		}

		[Test]
		public void TestFileCannotBeAccessed()
		{
			var fileName = GetUniqueNonExistingFileName();
			using (new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				var source = Create(fileName);
				_taskScheduler.Run(5);

				source.GetProperty(Properties.EmptyReason).Should().BeOfType<SourceCannotBeAccessed>();
				source.GetProperty(Properties.Created).Should().NotBe(DateTime.MinValue);
				source.GetProperty(Properties.Created).Should().Be(new FileInfo(fileName).CreationTimeUtc);
			}
		}
	}
}