using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Test.BusinessLogic.Plugins
{
	[TestFixture]
	public sealed class PluginInterfaceVersionAttributeTest
	{
		interface ITestPluginWithoutExplicitVersion
			: IPlugin
		{ }

		[PluginInterfaceVersion(42)]
		interface ITestPlugin
			: IPlugin
		{ }

		class TestlogAnalyserPlugin
			: ILogAnalyserPlugin
		{
			public AnalyserPluginId Id { get; }
			public ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
			{
				throw new System.NotImplementedException();
			}
		}

		[Test]
		public void TestGetLogAnalyserPlugin()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(ITestPluginWithoutExplicitVersion)).Should().Be(PluginInterfaceVersion.First);
		}

		[Test]
		public void TestGetLogAnalyserPluginVersionFromImplementation()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(TestlogAnalyserPlugin)).Should().Be(PluginInterfaceVersion.First);
		}

		[Test]
		public void TestGetTestPluginVersion()
		{
			PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(ITestPlugin)).Should().Be(new PluginInterfaceVersion(42));
		}

		[Test]
		public void TestGetTestPluginVersionWrongType()
		{
			new Action(() => PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(object)))
				.ShouldThrow<ArgumentException>();
		}
	}
}
