using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFilePropertyBufferTest
		: AbstractLogFilePropertiesTest
	{
		[Test]
		public void TestConstruction()
		{
			var buffer = new LogFilePropertyBuffer(LogFileProperties.Created);
			buffer.GetValue(LogFileProperties.Created).Should().Be(LogFileProperties.Created.DefaultValue);
		}

		protected override ILogFileProperties Create(params KeyValuePair<ILogFilePropertyDescriptor, object>[] properties)
		{
			var buffer = new LogFilePropertyBuffer(properties.Select(x => x.Key).ToArray());
			foreach (var pair in properties)
			{
				buffer.SetValue(pair.Key, pair.Value);
			}

			return buffer;
		}
	}
}