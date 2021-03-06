﻿using System.IO;
using System.Threading;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Tests;

namespace Tailviewer.Tests
{
	public sealed class SimplePluginLogSourceFactory
		: PluginLogSourceFactory
	{
		public SimplePluginLogSourceFactory(ITaskScheduler scheduler)
			: base(CreateServiceContainer(scheduler), null)
		{}

		private static IServiceContainer CreateServiceContainer(ITaskScheduler scheduler)
		{
			var container = new ServiceContainer();
			container.RegisterInstance<ITaskScheduler>(scheduler);
			var filesystem = new Filesystem(scheduler);
			container.RegisterInstance<IFilesystem>(filesystem);
			container.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			container.RegisterInstance<ILogEntryParserPlugin>(new SimpleLogEntryParserPlugin());
			container.RegisterInstance<IRawFileLogSourceFactory>(new StreamingTextLogSourceFactory(filesystem, scheduler));
			container.RegisterInstance<IPluginLoader>(new PluginRegistry());
			container.RegisterInstance<ILogSourceParserPlugin>(new ParsingLogSourceFactory(container));
			return container;
		}
	}
}
