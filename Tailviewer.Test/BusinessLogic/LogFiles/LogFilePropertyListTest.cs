using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
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

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue5()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceDoesNotExist);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);

			properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
		}

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue6()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue((ILogFilePropertyDescriptor)LogFileProperties.EmptyReason, ErrorFlags.SourceDoesNotExist);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);

			properties.SetValue((ILogFilePropertyDescriptor)LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
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