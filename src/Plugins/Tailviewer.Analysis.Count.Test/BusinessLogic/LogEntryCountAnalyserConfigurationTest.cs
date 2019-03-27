using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Analysis.Count.BusinessLogic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core;
using Tailviewer.Core.Settings;

namespace Tailviewer.Analysis.Count.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogEntryCountAnalyserConfigurationTest
	{
		[Test]
		public void TestIsEquivalent1()
		{
			var config = new LogEntryCountAnalyserConfiguration();
			config.IsEquivalent(null).Should().BeFalse();
			config.IsEquivalent(new Mock<ILogAnalyserConfiguration>().Object).Should().BeFalse();
		}

		[Test]
		public void TestIsEquivalent2()
		{
			var config1 = new LogEntryCountAnalyserConfiguration();
			var config2 = new LogEntryCountAnalyserConfiguration();
			config1.IsEquivalent(config2).Should().BeTrue();
			config2.IsEquivalent(config1).Should().BeTrue();
		}

		[Test]
		public void TestIsEquivalent3()
		{
			var config1 = new LogEntryCountAnalyserConfiguration();
			var config2 = new LogEntryCountAnalyserConfiguration
			{
				QuickFilters =
				{
					new QuickFilter
					{
						Value = "foo"
					}
				}
			};
			config1.IsEquivalent(config2).Should().BeFalse();
			config2.IsEquivalent(config1).Should().BeFalse();
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var config = new LogEntryCountAnalyserConfiguration();
			var actualConfig = Roundtrip(config);
			actualConfig.Should().NotBeNull();
			actualConfig.QuickFilters.Should().NotBeNull();
			actualConfig.QuickFilters.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripOneQuickFilter()
		{
			var id = QuickFilterId.CreateNew();
			var config = new LogEntryCountAnalyserConfiguration
			{
				QuickFilters =
				{
					new QuickFilter
					{
						Id = id,
						IgnoreCase = true,
						IsInverted = false,
						MatchType = FilterMatchType.WildcardFilter,
						Value = "dawawdawdaw"
					}
				}
			};
			var actualConfig = Roundtrip(config);
			actualConfig.Should().NotBeNull();
			actualConfig.QuickFilters.Should().NotBeNull();
			actualConfig.QuickFilters.Should().HaveCount(1);

			var quickFilter = actualConfig.QuickFilters.First();
			quickFilter.Id.Should().Be(id);
			quickFilter.IgnoreCase.Should().BeTrue();
			quickFilter.IsInverted.Should().BeFalse();
			quickFilter.MatchType.Should().Be(FilterMatchType.WildcardFilter);
			quickFilter.Value.Should().Be("dawawdawdaw");
		}

		private LogEntryCountAnalyserConfiguration Roundtrip(LogEntryCountAnalyserConfiguration config)
		{
			using (var stream = new MemoryStream())
			{
				var typeFactory = CreateTypeFactory();
				using (var writer = new Writer(stream, typeFactory))
				{
					config.Serialize(writer);
				}

				stream.Position = 0;
				Print(stream);
				stream.Position = 0;

				var reader = new Reader(stream, typeFactory);
				var actualConfig = new LogEntryCountAnalyserConfiguration();
				actualConfig.Deserialize(reader);
				return actualConfig;
			}
		}

		private ITypeFactory CreateTypeFactory()
		{
			var factory = new TypeFactory();
			factory.Add<QuickFilter>();
			factory.Add<QuickFilters>();
			factory.Add<QuickFilterId>();
			return factory;
		}

		private void Print(MemoryStream stream)
		{
			var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
			var content = reader.ReadToEnd();
			TestContext.Progress.WriteLine(content);
		}
	}
}