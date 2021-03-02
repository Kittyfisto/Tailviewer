using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api.Tests;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Settings;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.AutoUpdater
{
	[TestFixture]
	public sealed class AutoUpdaterTest
	{
		[Test]
		[LocalTest("Not intended to run on the build server")]
		public void TestQueryNewestVersion()
		{
			var actionCenter = new Mock<IActionCenter>();
			var updater = new Tailviewer.BusinessLogic.AutoUpdates.AutoUpdater(actionCenter.Object, new AutoUpdateSettings
			{
				CheckForUpdates = false
			});
			var newestVersion = updater.QueryNewestVersions();
			newestVersion.Should().NotBeNull("because we should be able to retrieve the latest version");
		}

		[Test]
		[LocalTest("Not intended to run on the build server")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/151")]
		public void TestDownloadNewestVersion()
		{
			var actionCenter = new Mock<IActionCenter>();
			var updater = new Tailviewer.BusinessLogic.AutoUpdates.AutoUpdater(actionCenter.Object, new AutoUpdateSettings
			{
				CheckForUpdates = false
			});
			var newestVersion = updater.QueryNewestVersions();
			new Action(() => updater.Download(newestVersion.Stable, newestVersion.StableAddress))
				.Should().NotThrow();
		}
	}
}
