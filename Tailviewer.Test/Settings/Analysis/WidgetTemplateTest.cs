using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class WidgetTemplateTest
	{
		[Test]
		public void TestClone1()
		{
			var template = new WidgetTemplate
			{
				Title = "Foobar"
			};
			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Title.Should().Be("Foobar");
			clone.Configuration.Should().BeNull();
		}

		[Test]
		public void TestClone2()
		{
			var viewConfiguration = new Mock<IWidgetConfiguration>();
			viewConfiguration.Setup(x => x.Clone()).Returns(() => new Mock<IWidgetConfiguration>().Object);
			var template = new WidgetTemplate
			{
				Configuration = viewConfiguration.Object
			};
			viewConfiguration.Verify(x => x.Clone(), Times.Never);

			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Title.Should().BeNull();
			clone.Configuration.Should().NotBeNull();
			clone.Configuration.Should().NotBeSameAs(viewConfiguration.Object);
			viewConfiguration.Verify(x => x.Clone(), Times.Once);
		}

		sealed class TestConfiguration
			: IWidgetConfiguration
		{
			public void Serialize(IWriter writer)
			{
				
			}

			public void Deserialize(IReader reader)
			{
				
			}

			public object Clone()
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void TestSerialize1()
		{
			var template = new WidgetTemplate
			{
				Id = WidgetId.CreateNew(),
				AnalyserId = AnalyserId.CreateNew(),
				Title = "dwankwadjkwad",
				Configuration = new TestConfiguration()
			};

			var actualTemplate = template.Roundtrip(typeof(TestConfiguration));
			actualTemplate.Should().NotBeNull();
			actualTemplate.Id.Should().Be(template.Id);
			actualTemplate.AnalyserId.Should().Be(template.AnalyserId);
			actualTemplate.Title.Should().Be(template.Title);
			actualTemplate.Configuration.Should().NotBeNull();
			actualTemplate.Configuration.Should().BeOfType<TestConfiguration>();
			actualTemplate.Configuration.Should().NotBeSameAs(template.Configuration);
		}

		[Test]
		[Description("Verifies that not being able to restore the configuration is NOT a problem")]
		public void TestSerialize2()
		{
			var template = new WidgetTemplate
			{
				Configuration = new TestConfiguration()
			};

			// We don't pass the type of the expected sub-types so the configuration
			// cannot be restored. This can happen when opening a template / snapshot
			// on an older installation or one that doesn't have a particular plugin.
			var actualTemplate = template.Roundtrip();
			actualTemplate.Should().NotBeNull();
			actualTemplate.Configuration.Should().BeNull();
		}
	}
}
