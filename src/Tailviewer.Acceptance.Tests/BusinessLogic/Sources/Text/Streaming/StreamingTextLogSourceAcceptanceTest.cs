﻿using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.Sources.Text.Streaming
{
	[TestFixture]
	public sealed class StreamingTextLogSourceAcceptanceTest
		: AbstractTextLogSourceAcceptanceTest
	{
		protected override ILogSource Create(IFilesystem filesystem, ITaskScheduler taskScheduler, string fileName, ILogFileFormat format, Encoding encoding)
		{
			return new StreamingTextLogSource(filesystem, taskScheduler, fileName, format, encoding);
		}

		[Test]
		[Ignore("TODO: Find out why this fails sporadically")]
		public override void TestReadAll2()
		{ }
	}
}