using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.SystemTests.Plugins.v0._8._0._680
{
	[TestFixture]
	public sealed class PluginCompatibilityTest
		: AbstractPluginCompatabilityTest
	{
		[SetUp]
		public void Setup()
		{
			// Tests which execute Tailviewer.exe interefere with each other:
			// We introduce some timeout in between those tests so they are more likely
			// to succeed (until a proper fix has been implemented).
			Thread.Sleep(TimeSpan.FromMilliseconds(500));
		}

		[Test]
		public void TestLoadFileFormatPlugin()
		{
			// CMD> Tailviewer.PluginCreator.exe FileFormatPlugin  Tailviewer.BusinessLogic.Plugins.IFileFormatPlugin 0.8.0.680-beta
			ShouldNotBeLoaded<IFileFormatPlugin>(new Version(0, 8, 0, 680), "FileFormatPlugin.0.0.tvp");
		}

		[Test]
		public void TestLoadFileFormatPluginAndLogFileImplementation()
		{
			// CMD> Tailviewer.PluginCreator.exe FileFormatPlugin+LogFile  Tailviewer.BusinessLogic.Plugins.IFileFormatPlugin,Tailviewer.BusinessLogic.LogFiles.ILogFile 0.8.0.680-beta
			ShouldNotBeLoaded<IFileFormatPlugin>(new Version(0, 8, 0, 680), "FileFormatPlugin+LogFile.0.0.tvp");
		}

		[Test]
		public void TestLoadDataSourceAnalyserPlugin()
		{
			// CMD> Tailviewer.PluginCreator.exe DataSourceAnalyserPlugin Tailviewer.BusinessLogic.Analysis.IDataSourceAnalyserPlugin 0.8.0.680-beta
			ShouldNotBeLoaded<IDataSourceAnalyserPlugin>(new Version(0, 8, 0, 680), "DataSourceAnalyserPlugin.0.0.tvp");
		}

		[Test]
		public void TestLoadDataSourceAnalyserPluginAndDataSourceAnalyserImplementation()
		{
			// CMD> Tailviewer.PluginCreator.exe DataSourceAnalyserPlugin+DataSourceAnalyser  Tailviewer.BusinessLogic.Analysis.IDataSourceAnalyserPlugin,Tailviewer.BusinessLogic.Analysis.IDataSourceAnalyser 0.8.0.680-beta
			ShouldNotBeLoaded<IDataSourceAnalyserPlugin>(new Version(0, 8, 0, 680), "DataSourceAnalyserPlugin+DataSourceAnalyser.0.0.tvp");
		}

		[Test]
		public void TestLoadLogAnalyserPlugin()
		{
			// CMD> LogAnalyserPlugin  Tailviewer.BusinessLogic.Analysis.ILogAnalyserPlugin 0.8.0.680-beta
			ShouldNotBeLoaded<ILogAnalyserPlugin>(new Version(0, 8, 0, 680), "LogAnalyserPlugin.0.0.tvp");
		}

		[Test]
		public void TestLoadLogAnalyserPluginAndLogAnalyserImplementation()
		{
			// CMD> LogAnalyserPlugin  Tailviewer.BusinessLogic.Analysis.ILogAnalyserPlugin 0.8.0.680-beta
			ShouldNotBeLoaded<ILogAnalyserPlugin>(new Version(0, 8, 0, 680), "LogAnalyserPlugin+LogAnalyser.0.0.tvp");
		}
	}
}
