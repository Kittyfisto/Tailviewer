using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests
{
	[TestFixture]
	public sealed class ApplicationTest
		: SystemtestBase
	{
		private string _installationPath;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			var dir = TestContext.CurrentContext.TestDirectory;
			var testName = TestContext.CurrentContext.Test.Name;
			_installationPath = Path.Combine(dir, testName);

			Clear(_installationPath);
			InstallInto(_installationPath);
		}

		private string ExecutableName => Path.Combine(_installationPath, "Tailviewer.exe");

		[Test]
		[LocalTest("Doesn't work on AppVeyor yet")]
		[Description("Verifies that Tailviewer only allows one instance to be executed at the same time")]
		public void TestOnlyOneApplication1()
		{
			using (var originalProcess = ProcessEx.Start("#1", ExecutableName))
			{
				// In this setup we assume that original process is the longer running
				// one and thus we'll wait for a few seconds before we spawn the 2nd process
				originalProcess.WaitForExit(5000).Should().BeFalse("because the first process should keep running");

				using (var newProcess = ProcessEx.Start("#2", ExecutableName))
				{
					// We expect this newly spawned process to kill itself
					// after having verified that the first process is in fact usable.
					newProcess.WaitForExit(5000)
						.Should()
						.BeTrue(
							"because this process should kill itself because the user is perfectly capable of working with the original process");
					newProcess.ExitCode.Should().Be(0, "because the 2nd process should exit gracefully");
				}

				originalProcess.WaitForExit(5000).Should().BeFalse("because the first process should still be running");
			}
		}
	}
}