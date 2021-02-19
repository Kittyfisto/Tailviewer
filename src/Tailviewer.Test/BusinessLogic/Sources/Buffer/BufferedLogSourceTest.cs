using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Buffer;

namespace Tailviewer.Test.BusinessLogic.Sources.Buffer
{
	[TestFixture]
	public sealed class BufferedLogSourceTest
		: AbstractLogSourceTest
	{
		private ManualTaskScheduler _taskScheduler;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
		}

		#region Overrides of AbstractLogSourceTest

		protected override ILogSource CreateEmpty()
		{
			var source = new InMemoryLogSource();
			return new BufferedLogSource(_taskScheduler, source, TimeSpan.Zero);
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			var source = new InMemoryLogSource(content);
			return new BufferedLogSource(_taskScheduler, source, TimeSpan.Zero);
		}

		[Ignore("This test doesn't make much sense")]
		public override void TestDisposeData()
		{}

		#endregion
	}
}
