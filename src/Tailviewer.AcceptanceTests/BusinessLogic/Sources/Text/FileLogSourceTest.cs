﻿using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test;

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
		public void TestFileCannotBeAccessed()
		{
			var fileName = GetUniqueNonExistingFileName();
			using (new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				var source = Create(fileName);
				_taskScheduler.Run(5);

				source.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
				source.GetProperty(GeneralProperties.Created).Should().NotBe(DateTime.MinValue);
				source.GetProperty(GeneralProperties.Created).Should().Be(new FileInfo(fileName).CreationTimeUtc);
			}
		}
	}
}