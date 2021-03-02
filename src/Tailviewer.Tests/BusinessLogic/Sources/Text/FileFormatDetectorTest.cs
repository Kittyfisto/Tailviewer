using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text;

namespace Tailviewer.Tests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class FileFormatDetectorTest
	{
		[Test]
		[Description("Verifies that the detector positions the stream to 0 bytes before it tries to ask the service for the format")]
		public void TestDetectStreamNotAt0()
		{
			using (var stream = new MemoryStream())
			{
				var data = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
				stream.Write(data, 0, data.Length);
				stream.Position = 4;

				var matcher = new SimpleLogFileFormatMatcher(null);

				var detector = new FileFormatDetector(matcher);
				detector.TryDetermineFormat("", stream, Encoding.Default);

				matcher.Header.Should().NotBeNull("because the matcher should have been called");
				matcher.Header.Length.Should().Be(data.Length, "because the detector should have first repositioned the stream to 0 and forwarded all first 512 bytes to the matcher");
			}
		}

		[Test]
		[Description("Verifies that the detector doesn't do anything anymore once one matcher claims a sure detection")]
		public void TestSkipDetectionAfterCertaintySure()
		{
			using (var stream = new MemoryStream())
			{
				var data = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
				stream.Write(data, 0, data.Length);
				stream.Position = 4;

				var matcher = new SimpleLogFileFormatMatcher(null, Certainty.None);

				var detector = new FileFormatDetector(matcher);
				detector.TryDetermineFormat("", stream, Encoding.Default).Should().Be(LogFileFormats.GenericText, "because our detector couldn't find a format so the fallback should be generic text");
				matcher.NumInvocations.Should().Be(1, "because the detector should try at least once");

				matcher.Format = LogFileFormats.Csv;
				matcher.Certainty = Certainty.Sure;
				detector.TryDetermineFormat("", stream, Encoding.Default).Should().Be(LogFileFormats.Csv, "because now our detector could find a match and this should have been forward instead of generic text");
				matcher.NumInvocations.Should().Be(2, "because the last invocation our matcher was uncertain and thus the detector should have asked again");

				detector.TryDetermineFormat("", stream, Encoding.Default).Should().Be(LogFileFormats.Csv);
				matcher.NumInvocations.Should().Be(2, "because our matcher was certain it is a CSV file so the detector shouldn't have bothered to ask the matcher again");
			}
		}
	}
}
