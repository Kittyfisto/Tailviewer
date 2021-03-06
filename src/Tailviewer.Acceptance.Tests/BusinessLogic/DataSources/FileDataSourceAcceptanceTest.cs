﻿using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Tests;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class FileDataSourceAcceptanceTest
	{
		private ManualTaskScheduler _scheduler;
		private PluginLogSourceFactory _logSourceFactory;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logSourceFactory = new SimplePluginLogSourceFactory(_scheduler);
		}
	}
}
