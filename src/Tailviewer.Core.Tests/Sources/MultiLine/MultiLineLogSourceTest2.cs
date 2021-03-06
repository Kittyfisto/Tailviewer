﻿using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources.MultiLine
{
	[TestFixture]
	public sealed class MultiLineLogSourceTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(IFilesystem filesystem, ITaskScheduler taskScheduler)
		{
			return new MultiLineLogSource(taskScheduler, new EmptyLogSource(), TimeSpan.Zero);
		}

		#endregion
	}
}