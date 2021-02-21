using System.IO;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;

namespace Tailviewer.Test.BusinessLogic.Sources.Text
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
	}
}
