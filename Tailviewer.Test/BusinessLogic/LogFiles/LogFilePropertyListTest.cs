using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFilePropertyListTest
		: AbstractLogFilePropertiesTest
	{
		[Test]
		public void TestConstruction()
		{
			var properties = new LogFilePropertyList(LogFileProperties.Created);
			properties.GetValue(LogFileProperties.Created).Should().Be(LogFileProperties.Created.DefaultValue);
		}

		protected override ILogFileProperties Create(params KeyValuePair<ILogFilePropertyDescriptor, object>[] properties)
		{
			var list = new LogFilePropertyList(properties.Select(x => x.Key).ToArray());
			foreach (var pair in properties)
			{
				list.SetValue(pair.Key, pair.Value);
			}

			return list;
		}
	}
}