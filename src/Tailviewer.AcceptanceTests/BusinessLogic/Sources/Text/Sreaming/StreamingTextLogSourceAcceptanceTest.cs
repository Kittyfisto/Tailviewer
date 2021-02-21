using System.Text;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Core.Sources.Text.Streaming;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text.Sreaming
{
	[TestFixture]
	public sealed class StreamingTextLogSourceAcceptanceTest
		: AbstractTextLogSourceAcceptanceTest
	{
		protected override ILogSource Create(ITaskScheduler taskScheduler, string fileName, ILogFileFormat format, Encoding encoding)
		{
			return new StreamingTextLogSource(taskScheduler, fileName, format, encoding);
		}
	}
}