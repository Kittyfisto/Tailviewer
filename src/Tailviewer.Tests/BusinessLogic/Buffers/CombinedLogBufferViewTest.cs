using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Tests.BusinessLogic.Buffers
{
	[TestFixture]
	public sealed class CombinedLogBufferViewTest
		: AbstractLogBufferTest
	{
		[Test]
		[Ignore("This test doesn't make sense for this implementation")]
		public override void TestEmptyConstruction1()
		{ }

		#region Overrides of AbstractReadOnlyLogBufferTest

		protected override IReadOnlyLogBuffer CreateEmptyReadOnly(IEnumerable<IColumnDescriptor> columns)
		{
			// The base test can do all the heavy lifting for us.
			// Let us split up the source columns into their own buffers for profit
			var sources = columns.Select(x => new LogBufferList(x)).ToList();
			return new CombinedLogBufferView(sources);
		}

		protected override IReadOnlyLogBuffer CreateReadOnly(IEnumerable<IReadOnlyLogEntry> entries)
		{
			return Create(entries);
		}

		#endregion

		#region Overrides of AbstractLogBufferTest

		protected override ILogBuffer Create(IEnumerable<IReadOnlyLogEntry> entries)
		{
			var columns = entries.SelectMany(x => x.Columns).Distinct().ToList();
			// The base test can do all the heavy lifting for us.
			// Let us split up the source columns into their own buffers and make each source
			// buffer hold into the data from that single column
			var sources = columns.Select(x =>
			{
				
				var buffer = new LogBufferList(x);
				buffer.AddRange(entries);
				return buffer;
			}).ToList();

			// And now we construct the combined view from the source again.
			// This way all source tests which operate on data from one more than one column
			// verify for us if our charade holds up.
			return new CombinedLogBufferView(sources);
		}

		#endregion
	}
}
